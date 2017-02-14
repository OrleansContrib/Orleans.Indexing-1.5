using Orleans.Concurrency;

namespace Orleans.Indexing
{
    /// <summary>
    /// The interface for AHashIndexPartitionedPerSiloBucketImpl<K, V> system target,
    /// which is created in order to guide Orleans to find the grain instances
    /// more efficiently.
    /// 
    /// Generic SystemTargets are not supported yet, and that's why the
    /// interface is non-generic.
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    //internal interface AHashIndexPartitionedPerSiloBucket<K, V> : ISystemTarget, HashIndexInterface<K, V> where V : IIndexableGrain
    [Unordered]
    internal interface AHashIndexPartitionedPerSiloBucket : ISystemTarget, HashIndexInterface<object, IIndexableGrain>
    {
    }
}
