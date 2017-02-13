using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// Defines the interface for range indexes
    /// </summary>
    /// <typeparam name="K">the type of indexed attribute for
    /// the range index</typeparam>
    /// <typeparam name="V">the type of grain interface that is
    /// being indexed</typeparam>
    [Unordered]
    public interface IRangeIndex<K,V> : IndexInterface<K,V> where V : IIndexableGrain
    {
    }
}
