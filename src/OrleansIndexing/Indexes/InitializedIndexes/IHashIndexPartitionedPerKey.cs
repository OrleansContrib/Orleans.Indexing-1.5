using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace Orleans.Indexing
{
    /// <summary>
    /// A simple implementation of a partitioned and persistent hash-index
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    [Serializable]
    public class IHashIndexPartitionedPerKey<K, V> : HashIndexPartitionedPerKey<K, V, IHashIndexPartitionedPerKeyBucket<K,V>>, InitializedIndex where V : class, IIndexableGrain
    {
        public IHashIndexPartitionedPerKey(string indexName, bool isUniqueIndex) : base(indexName, isUniqueIndex)
        {
        }
    }
}
