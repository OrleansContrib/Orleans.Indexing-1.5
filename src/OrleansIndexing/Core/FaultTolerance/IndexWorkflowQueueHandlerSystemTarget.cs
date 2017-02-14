using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    [Reentrant]
    internal class IndexWorkflowQueueHandlerSystemTarget : SystemTarget, IIndexWorkflowQueueHandler
    {
        private IIndexWorkflowQueueHandler _base;

        internal IndexWorkflowQueueHandlerSystemTarget(Type iGrainType, int queueSeqNum, SiloAddress silo, bool isDefinedAsFaultTolerantGrain) : base(IndexWorkflowQueueHandlerBase.CreateIndexWorkflowQueueHandlerGrainId(iGrainType, queueSeqNum), silo)
        {
            _base = new IndexWorkflowQueueHandlerBase(iGrainType, queueSeqNum, silo, isDefinedAsFaultTolerantGrain, this.AsWeaklyTypedReference());
        }

        public Task HandleWorkflowsUntilPunctuation(Immutable<IndexWorkflowRecordNode> workflowRecordsHead)
        {
            return _base.HandleWorkflowsUntilPunctuation(workflowRecordsHead);
        }

        public Task Initialize(IIndexWorkflowQueue oldParentSystemTarget)
        {
            throw new NotSupportedException();
        }
    }
}
