using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// This interface is a marker interface for marking
    /// all I-Index classes and grain interfaces
    /// </summary>
    public interface InitializedIndex
    {
    }
}
