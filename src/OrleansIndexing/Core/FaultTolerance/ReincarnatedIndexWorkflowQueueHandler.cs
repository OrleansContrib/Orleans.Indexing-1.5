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
    internal class ReincarnatedIndexWorkflowQueueHandler : Grain, IIndexWorkflowQueueHandler
    {
        private IIndexWorkflowQueueHandler _base;

        public override Task OnActivateAsync()
        {
            DelayDeactivation(ReincarnatedIndexWorkflowQueue.ACTIVE_FOR_A_DAY);
            return base.OnActivateAsync();
        }

        public Task Initialize(IIndexWorkflowQueue oldParentSystemTarget)
        {
            if (_base == null)
            {
                GrainReference oldParentSystemTargetRef = oldParentSystemTarget.AsWeaklyTypedReference();
                string[] parts = oldParentSystemTargetRef.GetPrimaryKeyString().Split('-');
                if (parts.Length != 2)
                {
                    throw new Exception("The primary key for IndexWorkflowQueueSystemTarget should only contain a single special character '-', while it contains multiple. The primary key is '" + oldParentSystemTargetRef.GetPrimaryKeyString() + "'");
                }

                Type grainInterfaceType = TypeUtils.ResolveType(parts[0]);
                int queueSequenceNumber = int.Parse(parts[1]);

                GrainReference thisRef = this.AsWeaklyTypedReference();
                _base = new IndexWorkflowQueueHandlerBase(grainInterfaceType, queueSequenceNumber, oldParentSystemTargetRef.SystemTargetSilo, true /*otherwise it shouldn't have reached here!*/, thisRef);
            }
            return TaskDone.Done;
        }

        public Task HandleWorkflowsUntilPunctuation(Immutable<IndexWorkflowRecordNode> workflowRecordsHead)
        {
            return _base.HandleWorkflowsUntilPunctuation(workflowRecordsHead);
        }
    }
}
