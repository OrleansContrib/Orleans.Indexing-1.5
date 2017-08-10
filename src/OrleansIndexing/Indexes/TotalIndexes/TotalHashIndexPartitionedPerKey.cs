using System;

namespace Orleans.Indexing
{
    /// <summary>
    /// A simple implementation of a partitioned and persistent hash-index
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    [Serializable]
    public class TotalHashIndexPartitionedPerKey<K, V> : HashIndexPartitionedPerKey<K, V, TotalHashIndexPartitionedPerKeyBucket<K,V>>, TotalIndex where V : class, IIndexableGrain
    {
        public TotalHashIndexPartitionedPerKey(string indexName, bool isUniqueIndex) : base(indexName, isUniqueIndex)
        {
        }
    }
}
