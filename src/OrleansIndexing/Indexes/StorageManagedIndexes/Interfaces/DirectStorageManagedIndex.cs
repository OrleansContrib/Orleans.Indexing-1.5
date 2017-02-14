using Orleans.Concurrency;

namespace Orleans.Indexing
{
    /// <summary>
    /// This is a marker interface for DirectStorageManagedIndex implementation classes
    /// </summary>
    public interface DirectStorageManagedIndex : IGrain
    {
    }

    /// <summary>
    /// The interface for DirectStorageManagedIndex<K, V> grain,
    /// which is created in order to guide Orleans to find the grain instances
    /// more efficiently.
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    [Unordered]
    public interface DirectStorageManagedIndex<K, V> : DirectStorageManagedIndex, HashIndexInterface<K, V> where V : IIndexableGrain
    {
    }
}
