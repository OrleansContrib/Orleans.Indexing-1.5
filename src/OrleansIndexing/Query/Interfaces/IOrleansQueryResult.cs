using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// Extension for the built-in <see cref="IObservable"/> and <see cref="IDisposable"/>
    /// allowing for Orleans specific operations, which represents the results of a query
    /// </summary>
    //interface IOrleansQueryResult : IEnumerable, IDisposable
    //{
    //}

    /// <summary>
    /// Extension for the built-in <see cref="IEnumerable{T}"/> and <see cref="IDisposable"/>
    /// allowing for Orleans specific operations, which represents the results of a query
    /// 
    /// IOrleansQueryResult contains the whole result of a query.
    /// </summary>
    /// <typeparam name="TGrain">the grain interface type, which is the
    /// type of elements in the query result</typeparam>
    public interface IOrleansQueryResult<out TGrain> : IEnumerable<TGrain>, IDisposable where TGrain : IIndexableGrain
    {
    }
}
