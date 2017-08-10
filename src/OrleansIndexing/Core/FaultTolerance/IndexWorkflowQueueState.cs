using Orleans.Runtime;
using System;

namespace Orleans.Indexing
{
    /// <summary>
    /// The persistent unit for storing the information for a IndexWorkflowQueueSystemTarget
    /// </summary>
    [Serializable]
    internal class IndexWorkflowQueueState : GrainState<IndexWorkflowQueueEntry>
    {
        public IndexWorkflowQueueState(GrainId g, SiloAddress silo) : base(new IndexWorkflowQueueEntry(g, silo))
        {
        }
    }

    /// <summary>
    /// All the information stored for a single IndexWorkflowQueueSystemTarget
    /// </summary>
    [Serializable]
    internal class IndexWorkflowQueueEntry
    {
        //updates that must be propagated to indexes.
        internal IndexWorkflowRecordNode WorkflowRecordsHead;

        internal GrainId QueueId;

        internal SiloAddress Silo;

        public IndexWorkflowQueueEntry(GrainId queueId, SiloAddress silo)
        {
            WorkflowRecordsHead = null;
            QueueId = queueId;
            Silo = silo;
        }
    }
}
