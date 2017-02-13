using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// The grain interface for the IndexableGrain and IndexableGrainNonFaultTolerant grains.
    /// </summary>
    public interface IIndexableGrain : IGrain
    {
    }
    public interface IIndexableGrain<TProperties> : IIndexableGrain
    {
    }
}
