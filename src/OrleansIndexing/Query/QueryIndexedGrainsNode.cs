using Orleans.Runtime;
using Orleans.Runtime.MembershipService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Linq.Expressions;
using Orleans.Streams;

namespace Orleans.Indexing
{
    /// <summary>
    /// Implements <see cref="IOrleansQueryable"/>
    /// </summary>
    public class QueryIndexedGrainsNode<TIGrain, TProperties> : QueryGrainsNode<TIGrain, TProperties> where TIGrain : IIndexableGrain
    {
        private string _indexName;

        private object _param;

        public QueryIndexedGrainsNode(IGrainFactory grainFactory, IStreamProvider streamProvider, string indexName, object param) : base(grainFactory, streamProvider)
        {
            _indexName = indexName;
            _param = param;
        }

        public override async Task<IOrleansQueryResult<TIGrain>> GetResults()
        {
            IndexInterface index = GetGrainFactory().GetIndex(_indexName, typeof(TIGrain));
            //Type indexType = index.GetType();
            //if(indexType.GetGenericTypeDefinition() == typeof(AHashIndexPartitionedPerSiloImpl<,>))
            //{
            //    indexType.GetMethod("SetGrainFactory").Invoke(index, new object[] { GetGrainFactory() });
            //}

            //the actual lookup for the query result to be streamed to the observer
            return (IOrleansQueryResult<TIGrain>) await index.Lookup(_param);
        }

        public override async Task ObserveResults(IAsyncBatchObserver<TIGrain> observer)
        {
            IndexInterface index = GetGrainFactory().GetIndex(_indexName, typeof(TIGrain));
            //Type indexType = index.GetType();
            //if(indexType.GetGenericTypeDefinition() == typeof(AHashIndexPartitionedPerSiloImpl<,>))
            //{
            //    indexType.GetMethod("SetGrainFactory").Invoke(index, new object[] { GetGrainFactory() });
            //}
            //a stream is created for the query result
            IAsyncStream<TIGrain> resultStream = GetStreamProvider().GetStream<TIGrain>(Guid.NewGuid(), IndexUtils.GetIndexGrainID(typeof(TIGrain), _indexName));

            IOrleansQueryResultStream<TIGrain> result = new OrleansQueryResultStream<TIGrain>(resultStream);
            
            //the observer is attached to the query result
            await result.SubscribeAsync(observer);
            
            //the actual lookup for the query result to be streamed to the observer
            await index.Lookup(result.Cast<IIndexableGrain>(), _param);
        }
    }
}
