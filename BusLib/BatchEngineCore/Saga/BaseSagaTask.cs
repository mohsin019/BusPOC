﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore.Saga
{
    public abstract class BaseSagaTask<T> : ITaskSaga<T>
    {
        ConcurrentDictionary<string, Action<ISagaTaskContext<T>>> _sagaStateDictionary =
            new ConcurrentDictionary<string, Action<ISagaTaskContext<T>>>();

        public BaseSagaTask()
        {
            DefineSagaStates();
        }

        protected abstract void DefineSagaStates();

        protected void DefineState(string name, Action<ISagaTaskContext<T>> action)
        {
            _sagaStateDictionary.AddOrUpdate(name, action, (s,a)=> action);            
        }

        internal protected virtual string GetNextState(ISagaTaskContext<T> context)
        {
            if (!string.IsNullOrWhiteSpace(context.NextState))
                return context.NextState;

            var nextState = _sagaStateDictionary.Keys.SkipWhile(s => s != context.State).ElementAtOrDefault(1);
            return nextState;
        }

        public virtual void Started(ISagaTaskContext<T> context)
        {
            
        }

        public virtual void Completed(ISagaTaskContext<T> context)
        {
            
        }
    }
}