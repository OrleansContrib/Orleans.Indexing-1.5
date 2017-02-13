using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// Defines the interface for hash-indexes
    /// </summary>
    /// <typeparam name="K">the type of key for the hash index</typeparam>
    /// <typeparam name="V">the type of grain interface that is
    /// being indexed</typeparam>
    [Unordered]
    public interface HashIndexInterface<K,V> : IndexInterface<K,V> where V : IIndexableGrain
    {
        /// <summary>
        /// A hash-index can be either unique or non-unique. If the user defines
        /// a hash-index as a unique hash-index, then the index guarantees 
        /// there is at most one value V in the index for each key K. This method
        /// determines whether this hash-index is a unique hash-index.
        /// </summary>
        /// <returns>true, if there should be at most one grain
        /// associated with each key, otherwise false</returns>
        //Task<bool> IsUnique();

        /// <summary>
        /// This method retrieves the unique result of a lookup into the
        /// hash-index
        /// </summary>
        /// <param name="key">the lookup key</param>
        /// <returns>the result of lookup into the hash-index</returns>
        [ReadOnly]
        [AlwaysInterleave]
        Task<V> LookupUnique(K key);
    }
}
