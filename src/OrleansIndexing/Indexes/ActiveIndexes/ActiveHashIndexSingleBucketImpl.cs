using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Providers;

namespace Orleans.Indexing
{
    /// <summary>
    /// A simple implementation of a single-grain in-memory hash-index
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    [StorageProvider(ProviderName = Constants.MEMORY_STORAGE_PROVIDER_NAME)]
    [Reentrant]
    public class ActiveHashIndexSingleBucketImpl<K, V> : HashIndexSingleBucket<K, V>, ActiveHashIndexSingleBucket<K, V> where V : class, IIndexableGrain
    {
        internal override IndexInterface<K,V> GetNextBucket()
        {
            var NextBucket = GrainFactory.GetGrain<ActiveHashIndexSingleBucket<K, V>>(IndexUtils.GetNextIndexBucketIdInChain(this.AsWeaklyTypedReference()));
            State.NextBucket = NextBucket.AsWeaklyTypedReference();
            return NextBucket;
        }
    }
}
