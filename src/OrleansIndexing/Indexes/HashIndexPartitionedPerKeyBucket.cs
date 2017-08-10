using Orleans.Concurrency;
using Orleans.Runtime;

namespace Orleans.Indexing
{
    /// <summary>
    /// A simple implementation of a single-bucket in-memory hash-index
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    [Reentrant]
    public abstract class HashIndexPartitionedPerKeyBucket<K, V> : HashIndexSingleBucket<K, V>, HashIndexPartitionedPerKeyBucketInterface<K, V> where V : class, IIndexableGrain
    {
        private static readonly Logger logger = LogManager.GetLogger(string.Format("HashIndexPartitionedPerKeyBucket<{0},{1}>", typeof(K).Name, typeof(V).Name), LoggerType.Grain);

        Logger getLogger()
        {
            return logger;
        }
    }
}
