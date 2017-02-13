using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Orleans.Indexing
{
    /// <summary>
    /// This class represents the whole result of a query.
    /// </summary>
    /// <typeparam name="TIGrain">type of grain for query result</typeparam>
    [Serializable]
    public class OrleansQueryResult<TIGrain> : IOrleansQueryResult<TIGrain> where TIGrain : IIndexableGrain
    {
        protected IEnumerable<TIGrain> _results;
        
        public OrleansQueryResult(IEnumerable<TIGrain> results)
        {
            _results = results;
        }

        public void Dispose()
        {
            _results = null;
        }

        public IEnumerator<TIGrain> GetEnumerator()
        {
            return _results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _results.GetEnumerator();
        }
    }
}
