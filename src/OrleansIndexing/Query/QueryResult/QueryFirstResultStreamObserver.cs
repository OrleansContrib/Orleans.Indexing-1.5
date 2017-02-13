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
    /// in order to watch the first result of a query
    /// </summary>
    /// <typeparam name="T">type of object that is being observed</typeparam>
    public class QueryFirstResultStreamObserver<T> : IAsyncObserver<T>
    {
        private Action<T> _action;
        public QueryFirstResultStreamObserver(Action<T> action)
        {
            _action = action;
        }

        public Task OnCompletedAsync()
        {
            return TaskDone.Done;
        }

        public Task OnErrorAsync(Exception ex)
        {
            throw ex;
        }

        public Task OnNextAsync(T item, StreamSequenceToken token = null)
        {
            if (_action != null)
            {
                _action(item);
                _action = null;
            }
            return TaskDone.Done;
        }
    }
}
