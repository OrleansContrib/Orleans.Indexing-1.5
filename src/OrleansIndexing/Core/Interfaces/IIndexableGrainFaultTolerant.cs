using Orleans;
using Orleans.Concurrency;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// This is a marker interface for marking fault-tolerant indexable grains
    /// </summary>
    public interface IIndexableGrainFaultTolerant
    {
    }
}
