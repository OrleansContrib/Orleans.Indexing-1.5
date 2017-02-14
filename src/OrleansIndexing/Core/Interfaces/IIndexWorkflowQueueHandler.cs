using Orleans.Concurrency;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// The interface for IndexWorkflowQueueSystemTarget system-target.
    /// </summary>
    [Unordered]
    internal interface IIndexWorkflowQueueHandler : ISystemTarget, IGrainWithStringKey
    {
        /// <summary>
        /// Accepts a linked list of work-flow records to handle until reaching a punctuation
        /// </summary>
        /// <param name="workflowRecordsHead">the head of work-flow record linked-list</param>
        Task HandleWorkflowsUntilPunctuation(Immutable<IndexWorkflowRecordNode> workflowRecordsHead);
        
        /// <summary>
        /// This method is called for initializing the ReincarnatedIndexWorkflowQueueHandler
        /// </summary>
        /// <param name="oldParentSystemTarget"></param>
        Task Initialize(IIndexWorkflowQueue oldParentSystemTarget);
    }
}