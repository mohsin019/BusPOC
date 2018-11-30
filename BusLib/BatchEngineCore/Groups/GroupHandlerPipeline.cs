﻿using BusLib.Core;

namespace BusLib.BatchEngineCore.Groups
{
    internal class GroupHandlerPipeline:Pipeline<GroupMessage>
    {
        public GroupHandlerPipeline(IStateManager stateManager, ILogger logger,
            IBatchEngineSubscribers batchEngineSubscribers) : base(new GroupCommandHandler(logger, batchEngineSubscribers, stateManager))
        {

        }
    }
}