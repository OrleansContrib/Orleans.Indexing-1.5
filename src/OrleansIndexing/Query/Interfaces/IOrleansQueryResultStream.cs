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
    //interface IOrleansQueryResultStream : IOrleansObservable, IDisposable
    //{
    //}

    /// <summary>
    /// Extension for the built-in <see cref="IObservable{T}"/> and <see cref="IDisposable"/>
    /// allowing for Orleans specific operations, which represents the results of a query
    /// 
    /// IOrleansQueryResult is both IAsyncObservable and IAsyncObservable at the same time,
    /// similar to IAsyncStream of Orleans.
    /// </summary>
    /// <typeparam name="TGrain">the grain interface type, which is the
    /// type of elements in the query result</typeparam>
    public interface IOrleansQueryResultStream<TGrain> : IAsyncObservable<TGrain>, IAsyncBatchObserver<TGrain>, IDisposable where TGrain : IIndexableGrain
    {
        //Task<TGrain> GetFirst();

        IOrleansQueryResultStream<Y> Cast<Y>() where Y : IIndexableGrain;
    }
}
