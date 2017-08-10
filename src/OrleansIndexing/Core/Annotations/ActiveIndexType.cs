using System;

namespace Orleans.Indexing
{
    /// <summary>
    /// The enumeration of all possible Active Index types in the system
    /// </summary>
    public enum ActiveIndexType
    {
        /// <summary>
        /// Represents a hash-index that comprises of a single bucket.
        /// 
        /// This type of index is not distributed and should be used
        /// with caution. The whole index should not have many entries,
        /// because it should be maintainable in a single silo.
        /// </summary>
        HashIndexSingleBucket,

        /// <summary>
        /// Represents a distributed hash-index, and each bucket maintains
        /// a single value for the hash of the key.
        /// </summary>
        HashIndexPartitionedByKeyHash,

        /// <summary>
        /// Represents a distributed hash-index, and each bucket is
        /// maintained by a silo.
        /// 
        /// PartitionedBySilo indexes are not supported for Total Indexes.
        /// </summary>
        HashIndexPartitionedBySilo
    }
}
