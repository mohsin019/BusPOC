﻿//using System;
//using System.Runtime.InteropServices;
//using System.Threading;
//using BusLib.BatchEngineCore;
//using BusLib.BatchEngineCore.Saga;
//using BusLib.BatchEngineCore.Volume;
//using BusLib.Core;
//using BusLib.Helper;

//namespace CQRsConsole.TestProcess
//{
//    public class TestEngine
//    {
//        IStateManager _stateManager;

//        void Start(string name)
//        {
//            var p=new UnitTaskProcess();
//            var volume = p.GetVolume(null);
//            //save state i.e. volume, sql or redis or mongodb etc

//            var type = p.VolumeDataType;
//            var isSaga = typeof(ITaskSaga<>).IsAssignableFrom(p.TaskActorType);
            
//            //todo resolve actor_type using ioc container

//            foreach (var i in volume)
//            {
//                var ctxt = SagaTaskContext.Create(i);

//            }

//        }
//    }

    


//    class SagaTaskContext
//    {
//        public static SagaTaskContext<T> Create<T>(T val)
//        {
//            return new SagaTaskContext< T>(val);
//        }
//    }
//    class SagaTaskContext<T>: ITaskContext
//    {
//        public SagaTaskContext(T val)
//        {
//            Data = val;
//        }
//        public DateTime UpdatedOn { get; }
//        public TaskCompletionStatus Status { get; }
//        public int RetryCount { get; }
//        public IProcessExecutionContext ProcessExecutionContext { get; }
//        public ITransaction Transaction { get; }
//        public CancellationToken CancellationToken { get; }
//        public T Data { get; }
//        public ITaskState State { get; }
//        public string PrevState { get; }
//        public string PreviousState { get; }
//        public string NextState { get; }
//        public ILogger Logger { get; }
//        public IDashboardService DashboardService { get; }
//        public ResultStatus Result { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//        public void Dispose()
//        {
            
//        }
//    }
//}