using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Providers;

namespace Orleans.Indexing
{
    /// <summary>
    /// A simple implementation of a single-grain persistent hash-index
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    [StorageProvider(ProviderName = Constants.INDEXING_STORAGE_PROVIDER_NAME)]
    [Reentrant]
    public class IHashIndexSingleBucketImpl<K, V> : HashIndexSingleBucket<K, V>, IHashIndexSingleBucket<K, V> where V : class, IIndexableGrain
    {
        internal override IndexInterface<K, V> GetNextBucket()
        {
            var NextBucket = GrainFactory.GetGrain<IHashIndexSingleBucketImpl<K, V>>(IndexUtils.GetNextIndexBucketIdInChain(this.AsWeaklyTypedReference()));
            State.NextBucket = NextBucket.AsWeaklyTypedReference();
            return NextBucket;
        }
    }
}
