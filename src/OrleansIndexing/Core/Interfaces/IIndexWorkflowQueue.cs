using Orleans.Concurrency;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Orleans.Indexing
{
    /// <summary>
    /// The interface for IndexWorkflowQueueSystemTarget system-target.
    /// </summary>
    [Unordered]
    internal interface IIndexWorkflowQueue : ISystemTarget, IGrainWithStringKey
    {
        /// <summary>
        /// Adds a workflowRecord, created by an indexable grain, to the queue
        /// </summary>
        Task AddToQueue(Immutable<IndexWorkflowRecord> workflowRecord);

        /// <summary>
        /// Adds a list of workflowRecords, created by an indexable grain, to the queue
        /// </summary>
        Task AddAllToQueue(Immutable<List<IndexWorkflowRecord>> workflowRecords);

        /// <summary>
        /// Removes a list of workflowRecords, created by an indexable grain, from the queue
        /// </summary>
        Task RemoveAllFromQueue(Immutable<List<IndexWorkflowRecord>> workflowRecords);

        /// <summary>
        /// If there is more work to do, hands it to the queue handler,
        /// otherwise sets the status of queue handler as idle.
        /// </summary>
        Task<Immutable<IndexWorkflowRecordNode>> GiveMoreWorkflowsOrSetAsIdle();

        /// <summary>
        /// Returns the list of work-flow records that are not completely processed
        /// and their ID is among the IDs in activeWorkflowsSet
        /// </summary>
        /// <param name="activeWorkflowsSet">the set of requested work-flow IDs</param>
        /// <returns>the work-flow records that their ID match one in the set of input</returns>
        Task<Immutable<List<IndexWorkflowRecord>>> GetRemainingWorkflowsIn(HashSet<Guid> activeWorkflowsSet);

        /// <summary>
        /// This method is called for initializing the ReincarnatedIndexWorkflowQueue
        /// </summary>
        /// <param name="oldParentSystemTarget"></param>
        Task Initialize(IIndexWorkflowQueue oldParentSystemTarget);
    }
}