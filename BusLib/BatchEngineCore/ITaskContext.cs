﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.BatchEngineCore
{
    public interface ITaskContext:IMessage
    {
        //todo: id, processId, correlationId, nodeId, 
        //todo: think about process companyId, branch, subtenant

        DateTime UpdatedOn { get; }

        TaskStatus Status { get; }

        int RetryCount { get; }

        IProcessExecutionContext ProcessExecutionContext { get; }
    }

    public interface ITaskContext<out T> : ITaskContext
    {
        T Data { get; }
    }

    public interface ISagaTaskContext<out T> : ITaskContext<T>
    {
        string State { get; }

        string PreviousState { get; }

        string NextState { get; }
                
    }
}
