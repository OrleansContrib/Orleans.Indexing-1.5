using System;

namespace Orleans.Indexing
{
    /// <summary>
    /// The enumeration of all possible index types in the system
    /// </summary>
    public enum IndexType
    {
        HashIndexSingleBucket,
        HashIndexPartitionedByKeyHash,
        HashIndexPartitionedBySilo
    }
}
