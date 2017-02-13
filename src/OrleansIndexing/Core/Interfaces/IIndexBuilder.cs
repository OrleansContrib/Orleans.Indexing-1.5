using Orleans;
using Orleans.Concurrency;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    public enum IndexBuilderStatus { Created, InProgress, Done };

    /// <summary>
    /// The grain interface for the IndexBuilder grain.
    /// </summary>
    public interface IIndexBuilder : IGrainWithStringKey
    {
        Task BuildIndex(string indexName, IndexInterface index, IndexMetaData indexMetaData, IIndexUpdateGenerator iUpdateGen);
        Task<bool> AddTombstone(IIndexableGrain removedGrain);
        Task<IndexBuilderStatus> GetStatus();
        Task<bool> IsDone();
    }

    /// <summary>
    /// The grain interface for the IndexBuilder grain,
    /// which build the indexes for a single grain interface.
    /// </summary>
    public interface IIndexBuilder<T> : IIndexBuilder where T : IIndexableGrain
    {
        Task<bool> AddTombstone(T removedGrain);
    }
}
