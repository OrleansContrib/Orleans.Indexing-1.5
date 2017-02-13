using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    /// <summary>
    /// This class casts IOrleansQueryResultStream{FromTP} to IOrleansQueryResultStream{ToTP}.
    /// 
    /// As IOrleansQueryResultStream{T} cannot be a covariant type (because it extends IAsyncObservable),
    /// this class is required when a conversion between two IOrleansQueryResultStream types is required.
    /// 
    /// It is not possible to subscribe to an instance of this class directly.
    /// One should use the original IOrleansQueryResultStream{FromTP} for subscription.
    /// </summary>
    /// <typeparam name="FromTP">type of grain for input IOrleansQueryResultStream</typeparam>
    /// <typeparam name="ToTP">type of grain for output IOrleansQueryResultStream</typeparam>

    [Serializable]
    public class OrleansQueryResultStreamCaster<FromTP, ToTP> : IOrleansQueryResultStream<ToTP> where FromTP : IIndexableGrain where ToTP : IIndexableGrain
    {
        protected IOrleansQueryResultStream<FromTP> _stream;
        
        // Accept a queryResult instance which we shall observe
        public OrleansQueryResultStreamCaster(IOrleansQueryResultStream<FromTP> stream)
        {
            _stream = stream;
        }

        public IOrleansQueryResultStream<TOGrain> Cast<TOGrain>() where TOGrain : IIndexableGrain
        {
            if (typeof(TOGrain) == typeof(FromTP)) return (IOrleansQueryResultStream<TOGrain>)_stream;
            return new OrleansQueryResultStreamCaster<FromTP, TOGrain>(_stream);
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public Task OnCompletedAsync()
        {
            return _stream.OnCompletedAsync();
        }

        public Task OnErrorAsync(Exception ex)
        {
            return _stream.OnErrorAsync(ex);
        }

        public Task OnNextAsync(ToTP item, StreamSequenceToken token = null)
        {
            return _stream.OnNextAsync(item.AsReference<FromTP>(), token);
        }

        public Task OnNextBatchAsync(IEnumerable<ToTP> batch, StreamSequenceToken token = null)
        {
            return Task.WhenAll(batch.Select(item => (_stream.OnNextAsync(item.AsReference<FromTP>(), token))));
            //TODO: replace with the code below, as soon as stream.OnNextBatchAsync is supported.
            //return _stream.OnNextBatchAsync(batch.Select(x => x.AsReference<FromTP>), token); //not supported yet!
        }

        public Task<StreamSubscriptionHandle<ToTP>> SubscribeAsync(IAsyncObserver<ToTP> observer)
        {
            throw new NotSupportedException();
        }

        public Task<StreamSubscriptionHandle<ToTP>> SubscribeAsync(IAsyncObserver<ToTP> observer, StreamSequenceToken token, StreamFilterPredicate filterFunc = null, object filterData = null)
        {
            throw new NotSupportedException();
        }
    }
}
