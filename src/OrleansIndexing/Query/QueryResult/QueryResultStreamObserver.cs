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
    /// This class is used for creating IAsyncBatchObserver
    /// in order to watch the result of a query
    /// </summary>
    /// <typeparam name="T">type of objects that are being observed</typeparam>
    public class QueryResultStreamObserver<T> : IAsyncBatchObserver<T>
    {
        private Func<T, Task> _onNext;
        private Func<Task> _onCompleted;
        public QueryResultStreamObserver(Func<T, Task> onNext, Func<Task> onCompleted = null)
        {
            _onNext = onNext;
            _onCompleted = onCompleted;
        }

        public Task OnCompletedAsync()
        {
            if (_onCompleted != null) return _onCompleted();
            return TaskDone.Done;
        }

        public Task OnErrorAsync(Exception ex)
        {
            throw ex;
        }

        public Task OnNextAsync(T item, StreamSequenceToken token = null)
        {
            return _onNext(item);
        }

        public Task OnNextBatchAsync(IEnumerable<T> batch, StreamSequenceToken token = null)
        {
            return Task.WhenAll(batch.Select(item => _onNext(item)));
        }
    }
}
