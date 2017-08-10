using System;

namespace Orleans.Indexing
{
    /// <summary>
    /// A simple implementation of a partitioned in-memory hash-index
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    [Serializable]
    public class ActiveHashIndexPartitionedPerKey<K, V> : HashIndexPartitionedPerKey<K, V, ActiveHashIndexPartitionedPerKeyBucket<K,V>> where V : class, IIndexableGrain
    {
        public ActiveHashIndexPartitionedPerKey(string indexName, bool isUniqueIndex) : base(indexName, isUniqueIndex)
        {
        }
    }
}
