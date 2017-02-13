using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    public enum IndexStatus { /*Created,*/ UnderConstruction, Available, Disposed }

    /// <summary>
    /// This interface defines the functionality
    /// that is required for an index implementation.
    /// </summary>
    [Unordered]
    public interface IndexInterface
    {
        /// <summary>
        /// This method applies a given update to the current index.
        /// </summary>
        /// <param name="updatedGrain">the grain that issued the update</param>
        /// <param name="iUpdate">contains the data for the update</param>
        /// <param name="isUnique">whether this is a unique index that we are updating</param>
        /// <param name="siloAddress">The address of the silo where the grain resides.</param>
        /// <returns>true, if the index update was successful, otherwise false</returns>
        [AlwaysInterleave]
        Task<bool> DirectApplyIndexUpdate(IIndexableGrain updatedGrain, Immutable<IMemberUpdate> iUpdate, bool isUnique, IndexMetaData idxMetaData, SiloAddress siloAddress = null);


        /// <summary>
        /// This method applies a batch of given updates to the current index.
        /// </summary>
        /// <param name="iUpdates">a mapping from the grains that issued the
        /// updates to the list of actual update information</param>
        /// <param name="isUnique">whether this is a unique index that we are updating</param>
        /// <param name="siloAddress">The address of the silo where the grain resides.</param>
        /// <returns>true, if the index update was successful, otherwise false</returns>
        [AlwaysInterleave]
        Task<bool> DirectApplyIndexUpdateBatch(Immutable<IDictionary<IIndexableGrain, IList<IMemberUpdate>>> iUpdates, bool isUnique, IndexMetaData idxMetaData, SiloAddress siloAddress = null);

        /// <summary>
        /// Disposes of the index and removes all the data stored
        /// for the index. This method is called before removing
        /// the index from index registry
        /// </summary>
        [AlwaysInterleave]
        Task Dispose();

        /// <summary>
        /// Determines whether the index is available for lookup
        /// </summary>
        [ReadOnly]
        [AlwaysInterleave]
        Task<bool> IsAvailable();
    }

    /// <summary>
    /// This is the typed variant of IndexInterface, which is assumed to be 
    /// the root interface for the index implementations.
    /// </summary>
    [Unordered]
    public interface IndexInterface<K,V> : IndexInterface where V : IIndexableGrain
    {
    }
}
