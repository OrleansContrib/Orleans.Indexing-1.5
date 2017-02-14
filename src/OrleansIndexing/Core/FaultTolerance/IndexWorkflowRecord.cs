using System;
using System.Collections.Generic;

namespace Orleans.Indexing
{
    /// <summary>
    /// Each workflowRecord contains the following information:
    ///    - workflowID: grainID + a sequence number
    ///    - memberUpdates: the updated values of indexed fields
    /// </summary>
    [Serializable]
    internal class IndexWorkflowRecord
    {
        /// <summary>
        /// The grain being indexes,
        /// which its ID is the first part of the workflowID
        /// </summary>
        internal IIndexableGrain Grain { get; private set; }

        /// <summary>
        /// The sequence number of update on the Grain,
        /// which is the second part of the workflowID
        /// </summary>
        internal Guid WorkflowId { get; private set; }

        /// <summary>
        /// The list of updates to all indexes of the Grain
        /// </summary>
        internal IDictionary<string, IMemberUpdate> MemberUpdates { get; private set; }

        internal IndexWorkflowRecord(Guid workflowId, IIndexableGrain grain, IDictionary<string, IMemberUpdate> memberUpdates)
        {
            Grain = grain;
            WorkflowId = workflowId;
            MemberUpdates = memberUpdates;
        }

        public override bool Equals(object other)
        {
            var otherW = other as IndexWorkflowRecord;
            return otherW != null ? WorkflowId.Equals(otherW.WorkflowId) : false;
        }

        public override int GetHashCode()
        {
            return WorkflowId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("<Grain: {0}, WorkflowId: {1}>", Grain, WorkflowId);
        }
    }
}