using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// This class represents the result of a query.
    /// 
    /// OrleansQueryResultStream is actually a stream of results
    /// that can be observed by its client.
    /// </summary>
    /// <typeparam name="TIGrain">type of grain for query result</typeparam>
    [Serializable]
    public class OrleansQueryResultStream<TIGrain> : IOrleansQueryResultStream<TIGrain> where TIGrain : IIndexableGrain
    {
        //Currently, the whole result is stored here, but it is
        //just a simple implementation. This implementation should
        //be replaced with a more sophisticated approach to asynchronously
        //read the results on demand

        protected IAsyncStream<TIGrain> _stream;

        // Accept a queryResult instance which we shall observe
        public OrleansQueryResultStream(IAsyncStream<TIGrain> stream)
        {
            _stream = stream;
        }

        public IOrleansQueryResultStream<TOGrain> Cast<TOGrain>() where TOGrain : IIndexableGrain
        {
            return new OrleansQueryResultStreamCaster<TIGrain, TOGrain>(this);
        }

        public void Dispose()
        {
            _stream = null;
        }

        public Task OnCompletedAsync()
        {
            return _stream.OnCompletedAsync();
        }

        public Task OnErrorAsync(Exception ex)
        {
            return _stream.OnErrorAsync(ex);
        }

        public virtual Task OnNextAsync(TIGrain item, StreamSequenceToken token = null)
        {
            return _stream.OnNextAsync(item, token);
        }

        public virtual Task OnNextBatchAsync(IEnumerable<TIGrain> batch, StreamSequenceToken token = null)
        {
            return Task.WhenAll(batch.Select(item => _stream.OnNextAsync(item, token)));
            //TODO: replace with the code below, as soon as stream.OnNextBatchAsync is supported.
            //return _stream.OnNextBatchAsync(batch, token); //not supported yet!
        }

        public Task<StreamSubscriptionHandle<TIGrain>> SubscribeAsync(IAsyncObserver<TIGrain> observer)
        {
            return _stream.SubscribeAsync(observer);
        }

        public Task<StreamSubscriptionHandle<TIGrain>> SubscribeAsync(IAsyncObserver<TIGrain> observer, StreamSequenceToken token, StreamFilterPredicate filterFunc = null, object filterData = null)
        {
            return _stream.SubscribeAsync(observer, token, filterFunc, filterData);
        }
    }
}
