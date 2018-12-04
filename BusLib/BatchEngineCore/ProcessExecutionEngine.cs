﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.Volume;
using BusLib.Core;
using BusLib.Helper;
using BusLib.PipelineFilters;

namespace BusLib.BatchEngineCore
{
    //pipeline
    class ProcessVolumeRequestHandler: IHandler<ProcessExecutionContext>
    {
        List<IBaseProcess> _registeredProcesses=new List<IBaseProcess>(); // todo: scan all assemblies for implemented processes
        private ICacheAside _cacheAside;
        ManualResetEvent _pauseHandler=new ManualResetEvent(true);
        private IFrameworkLogger _logger;
        private readonly IStateManager _stateManager;

        readonly ConcurrentDictionary<int, Pipeline<IProcessExecutionContext>> _processPipelines = new ConcurrentDictionary<int, Pipeline<IProcessExecutionContext>>();
        private int _delayInRetries = 3000; //todo

        void Execute(ProcessExecutionContext context)
        {
            _logger.Trace(context.GetFormattedLogMessage("Volume request received"));
            _pauseHandler.WaitOne();

            var processKey = context.ProcessState.ProcessKey;
            var pipeline = GetPipeline(processKey);

            if (pipeline == null)
            {
                var error = context.GetFormattedLogMessage("Volume handler not found");
                _logger.Error(error);
                context.MarkAsError(error);
                return;
            }

            _logger.Trace(context.GetFormattedLogMessage("Volume request sending to pipeline"));

            try
            {
                pipeline.Invoke(context);
            }
            catch (Exception e)
            {
                var error = context.GetFormattedLogMessage("Error generating volume", e);
                _logger.Error(error);
                context.MarkAsError(error);
            }

            //var t = typeof(GenericProcessHandler<>);
            //var gType = t.MakeGenericType(processInstance.VolumeDataType);
            //var handler = Activator.CreateInstance(gType, processInstance, context);

            ////update start date

            //if( processInstance.VolumeDataType == typeof(int))
            //{
            //    var pr = GenericProcessHandler.Create(processInstance);

            //    //send to query pipeline
            //    IBaseProcess<int> p = (IBaseProcess<int>)processInstance;
            //    Bus.Instance.QueryAction(()=> p.GetVolume(context), HandleVolume) //IEnumerable<int>
            //    ;
            //}
            //else
            //{
            //    //todo
            //}

        }
        
        Pipeline<IProcessExecutionContext> GetPipeline(int processKey)
        {
            var pipeLine = _processPipelines.GetOrAdd(processKey, key =>
            {
                var process = _registeredProcesses.FirstOrDefault(p => p.ProcessKey == processKey);
                if (process == null)
                    return null;
                var processConfig = _cacheAside.GetProcessConfiguration(key);
                var maxVolumeRetries = processConfig.MaxVolumeRetries;

                Pipeline<IProcessExecutionContext> pipeline = new Pipeline<IProcessExecutionContext>(new VolumeGenerator(process, _stateManager));
                
                if (maxVolumeRetries != 0) // -1 means unlimited retries
                {
                    pipeline.RegisterFeatureDecorator(
                        new RetryFeatureHandler<IProcessExecutionContext>(maxVolumeRetries,
                            _delayInRetries, _logger));
                }

                var duplicateCheckFilter = new DuplicateCheckFilter<IProcessExecutionContext,int>( (context => context.ProcessState.Id), $"VolumeGenerator{processKey}", _logger);
                pipeline.RegisterFeatureDecorator(duplicateCheckFilter);
                var refr=new WeakReference<DuplicateCheckFilter<IProcessExecutionContext, int>>(duplicateCheckFilter); //todo
                Bus.Instance.EventAggregator.Subscribe<ProcessGroupRemovedMessage>(msg =>
                {
                    if (refr.TryGetTarget(out DuplicateCheckFilter<IProcessExecutionContext, int> filt))
                    {
                        filt.Cleanup(msg.Group.ProcessEntities.Select(r => r.Id));
                    }
                });
                pipeline.RegisterFeatureDecorator(new ThrottlingFilter<IProcessExecutionContext>(1, _logger));

                return pipeline;
            });
            return pipeLine;
        }

        public void Handle(ProcessExecutionContext message)
        {
            Execute(message);
        }

        class VolumeGenerator:IHandler<IProcessExecutionContext>
        {
            private readonly IBaseProcess _process;
            private readonly IStateManager _stateManager;

            public VolumeGenerator(IBaseProcess process, IStateManager stateManager)
            {
                _process = process;
                _stateManager = stateManager;
            }

            public void Handle(IProcessExecutionContext message)
            {
                message.Logger.Trace("Volume handler ");

                IVolumeHandler volumeHandler = null; //todo initialize based on configuration or fixed?

                var processState = _stateManager.GetProcessById(message.ProcessState.Id);
                if (ProcessStatus.New.Id != processState.Status.Id || processState.IsStopped)
                {
                    message.Logger.Warn("Cannot generate volume if process is not 'New' or process is Stopped");
                    return;
                }
                _process.HandleVolume(volumeHandler, message);
                message.Logger.Info("Volume generated successfully ");

                if (_process is IHasSupportingData processWithSupportingData)
                {
                    message.Logger.Trace("Initializing Supporting Data ");

                    //initializer process wise cache data
                    Robustness.Instance.SafeCallWithRetry(() => processWithSupportingData.InitializeSupportingData(message), 3, 1000, message.Logger);
                }

                message.Logger.Trace("Marking process as Generated");
                message.ProcessState.MarkAsVolumeGenerated();
                
            }

            public void Dispose()
            {
            }
        }

        //private void HandleVolume(IEnumerable<int> obj)
        //{
        //    throw new NotImplementedException();
        //}

        //class GenericProcessHandler
        //{
        //    internal static GenericProcessHandler<T> Create<T>(T it)
        //    {
        //        return new GenericProcessHandler<T>();
        //    }
        //}

        //class GenericProcessHandler<T>
        //{
        //    IBaseProcess<T> _process;
        //    IProcessExecutionContext _context;
        //    IBaseProcess<T> GetProcess(string key)
        //    {
        //        IEnumerable<T> res = _process.GetVolume(_context);
        //        //todo persist
        //        throw new NotImplementedException();
        //    }
        //}

        public void Dispose()
        {
            _pauseHandler?.Dispose();
        }
    }

}
