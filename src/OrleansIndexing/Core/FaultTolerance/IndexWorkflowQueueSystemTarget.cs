using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// To minimize the number of RPCs, we process index updates for each grain
    /// on the silo where the grain is active. To do this processing, each silo
    /// has one or more IndexWorkflowQueueSystemTarget system-targets for each grain class,
    /// up to the number of hardware threads. A system-target is a grain that
    /// belongs to a specific silo.
    /// + Each of these system-targets has a queue of workflowRecords, which describe
    ///   updates that must be propagated to indexes.Each workflowRecord contains
    ///   the following information:
    ///    - workflowID: grainID + a sequence number
    ///    - memberUpdates: the updated values of indexed fields
    ///  
    ///   Ordinarily, these workflowRecords are for grains that are active on
    ///   IndexWorkflowQueueSystemTarget's silo. (This may not be true for short periods when
    ///   a grain migrates to another silo or after the silo recovers from failure).
    /// 
    /// + The IndexWorkflowQueueSystemTarget grain Q has a dictionary updatesOnWait is an
    ///   in-memory dictionary that maps each grain G to the workflowRecords for G
    ///   that are waiting for be updated
    /// </summary>
    [StorageProvider(ProviderName = Constants.INDEXING_WORKFLOWQUEUE_STORAGE_PROVIDER_NAME)]
    [Reentrant]
    internal class IndexWorkflowQueueSystemTarget : SystemTarget, IIndexWorkflowQueue
    {
        private IndexWorkflowQueueBase _base;

        internal IndexWorkflowQueueSystemTarget(Type grainInterfaceType, int queueSequenceNumber, SiloAddress silo, bool isDefinedAsFaultTolerantGrain) : base(IndexWorkflowQueueBase.CreateIndexWorkflowQueueGrainId(grainInterfaceType, queueSequenceNumber), silo)
        {
            GrainReference thisRef = this.AsWeaklyTypedReference();
            _base = new IndexWorkflowQueueBase(grainInterfaceType, queueSequenceNumber, silo, isDefinedAsFaultTolerantGrain, ((ISystemTargetBase)this).GrainId, thisRef);
        }

        public Task AddAllToQueue(Immutable<List<IndexWorkflowRecord>> workflowRecords)
        {
            return _base.AddAllToQueue(workflowRecords);
        }

        public Task AddToQueue(Immutable<IndexWorkflowRecord> workflowRecord)
        {
            return _base.AddToQueue(workflowRecord);
        }

        public Task<Immutable<List<IndexWorkflowRecord>>> GetRemainingWorkflowsIn(HashSet<Guid> activeWorkflowsSet)
        {
            return _base.GetRemainingWorkflowsIn(activeWorkflowsSet);
        }

        public Task<Immutable<IndexWorkflowRecordNode>> GiveMoreWorkflowsOrSetAsIdle()
        {
            return _base.GiveMoreWorkflowsOrSetAsIdle();
        }

        public Task RemoveAllFromQueue(Immutable<List<IndexWorkflowRecord>> workflowRecords)
        {
            return _base.RemoveAllFromQueue(workflowRecords);
        }

        public Task Initialize(IIndexWorkflowQueue oldParentSystemTarget)
        {
            throw new NotSupportedException();
        }
    }
}
