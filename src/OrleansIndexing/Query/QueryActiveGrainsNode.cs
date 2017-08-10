using System;
using System.Threading.Tasks;
using Orleans.Streams;

namespace Orleans.Indexing
{
    /// <summary>
    /// The query class for querying all active grains of a given type
    /// </summary>
    public class QueryActiveGrainsNode<TIGrain, TProperties> : QueryGrainsNode<TIGrain, TProperties> where TIGrain : IIndexableGrain
    {
        public QueryActiveGrainsNode(IGrainFactory gf, IStreamProvider streamProvider) : base(gf, streamProvider)
        {
        }

        public override Task<IOrleansQueryResult<TIGrain>> GetResults()
        {
            throw new NotSupportedException(string.Format("Traversing over all the active grains of {0} is not supported.", typeof(TIGrain)));
        }

        public override Task ObserveResults(IAsyncBatchObserver<TIGrain> observer)
        {
            throw new NotSupportedException(string.Format("Traversing over all the active grains of {0} is not supported.", typeof(TIGrain)));
        }
    }
}
