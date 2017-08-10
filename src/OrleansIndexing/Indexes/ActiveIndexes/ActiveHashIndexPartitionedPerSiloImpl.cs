using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace Orleans.Indexing
{
    /// <summary>
    /// A simple implementation of a single-grain in-memory hash-index
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    [Reentrant]
    //[StatelessWorker]
    //TODO: because of a bug in OrleansStreams, this grain cannot be StatelessWorker. It should be fixed later.
    //TODO: basically, this class does not even need to be a grain, but it's not possible to call a SystemTarget from a non-grain
    public class ActiveHashIndexPartitionedPerSiloImpl<K, V> : Grain, ActiveHashIndexPartitionedPerSilo<K, V> where V : class, IIndexableGrain
    {
        private static readonly Logger logger = LogManager.GetLogger(string.Format("ActiveHashIndexPartitionedPerSiloImpl<{0},{1}>", typeof(K).Name, typeof(V).Name), LoggerType.Grain);

        private IndexStatus _status;
        public static void InitPerSilo(Silo silo, string indexName, bool isUnique)
        {
            silo.RegisterSystemTarget(new ActiveHashIndexPartitionedPerSiloBucketImpl(
                indexName,
                GetGrainID(indexName),
                silo.SiloAddress
            ));
        }

        public override Task OnActivateAsync()
        {
            _status = IndexStatus.Available;

            return base.OnActivateAsync();
        }

        /// <summary>
        /// DirectApplyIndexUpdateBatch is not supported on ActiveHashIndexPartitionedPerSiloImpl,
        /// because it will be skipped via IndexExtensions.DirectApplyIndexUpdateBatch
        /// </summary>
        public Task<bool> DirectApplyIndexUpdateBatch(Immutable<IDictionary<IIndexableGrain, IList<IMemberUpdate>>> iUpdates, bool isUnique, IndexMetaData idxMetaData, SiloAddress siloAddress = null)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// DirectApplyIndexUpdate is not supported on ActiveHashIndexPartitionedPerSiloImpl,
        /// because it will be skipped via IndexExtensions.ApplyIndexUpdate
        /// </summary>
        public Task<bool> DirectApplyIndexUpdate(IIndexableGrain g, Immutable<IMemberUpdate> iUpdate, bool isUniqueIndex, IndexMetaData idxMetaData, SiloAddress siloAddress)
        {
            throw new NotSupportedException();
        }

        private static GrainId GetGrainID(string indexName)
        {
            return GrainId.GetSystemTargetGrainId(Constants.HASH_INDEX_PARTITIONED_PER_SILO_BUCKET_SYSTEM_TARGET_TYPE_CODE,
                                               IndexUtils.GetIndexGrainID(typeof(V), indexName));
        }

        public Task<bool> IsUnique()
        {
            return Task.FromResult(false);
        }

        public async Task<V> LookupUnique(K key)
        {
            var result = new OrleansFirstQueryResultStream<V>();
            var taskCompletionSource = new TaskCompletionSource<V>();
            Task<V> tsk = taskCompletionSource.Task;
            Action<V> responseHandler = taskCompletionSource.SetResult;
            await result.SubscribeAsync(new QueryFirstResultStreamObserver<V>(responseHandler));
            await Lookup(result, key);
            return await tsk;
        }

        public async Task Dispose()
        {
            _status = IndexStatus.Disposed;
            //get all silos
            Dictionary<SiloAddress, SiloStatus> hosts = await SiloUtils.GetHosts(true);
            var numHosts = hosts.Keys.Count;

            Task[] disposeToSilos = new Task[numHosts];

            int i = 0;
            IList<IOrleansQueryResultStream<V>> result = new List<IOrleansQueryResultStream<V>>();
            GrainId grainID = GetGrainID(IndexUtils.GetIndexNameFromIndexGrain(this));
            foreach (SiloAddress siloAddress in hosts.Keys)
            {
                //dispose the index on each silo
                disposeToSilos[i] = InsideRuntimeClient.Current.InternalGrainFactory.GetSystemTarget<ActiveHashIndexPartitionedPerSiloBucket>(
                    grainID,
                    siloAddress
                ).Dispose();
                ++i;
            }
            await Task.WhenAll(disposeToSilos);
        }

        public Task<bool> IsAvailable()
        {
            return Task.FromResult(_status == IndexStatus.Available);
        }

        async Task<IOrleansQueryResult<IIndexableGrain>> IndexInterface.Lookup(object key)
        {
            if (logger.IsVerbose) logger.Verbose("Eager index lookup called for key = {0}", key);

            //get all silos
            Dictionary<SiloAddress, SiloStatus> hosts = await SiloUtils.GetHosts(true);

            IEnumerable<IIndexableGrain>[] queriesToSilos = await Task.WhenAll(GetResultQueries(hosts, key));
            
            return new OrleansQueryResult<V>(queriesToSilos.SelectMany(res => res.Select(e => e.AsReference<V>())).ToList());
        }

        public async Task<IOrleansQueryResult<V>> Lookup(K key)
        {
            return (IOrleansQueryResult<V>) await ((IndexInterface)this).Lookup(key);
        }

        private ISet<Task<IOrleansQueryResult<IIndexableGrain>>> GetResultQueries(Dictionary<SiloAddress, SiloStatus> hosts, object key)
        {
            ISet<Task<IOrleansQueryResult<IIndexableGrain>>> queriesToSilos = new HashSet<Task<IOrleansQueryResult<IIndexableGrain>>>();

            int i = 0;
            GrainId grainID = GetGrainID(IndexUtils.GetIndexNameFromIndexGrain(this));
            foreach (SiloAddress siloAddress in hosts.Keys)
            {
                //query each silo
                queriesToSilos.Add(InsideRuntimeClient.Current.InternalGrainFactory.GetSystemTarget<ActiveHashIndexPartitionedPerSiloBucket>(
                    grainID,
                    siloAddress
                ).Lookup(/*result, */key)); //TODO: because of a bug in OrleansStream, a SystemTarget cannot work with streams. It should be fixed later.
                ++i;
            }

            return queriesToSilos;
        }

        public Task Lookup(IOrleansQueryResultStream<V> result, K key)
        {
            return ((IndexInterface)this).Lookup(result.Cast<IIndexableGrain>(), key);
        }

        async Task IndexInterface.Lookup(IOrleansQueryResultStream<IIndexableGrain> result, object key)
        {
            if (logger.IsVerbose) logger.Verbose("Streamed index lookup called for key = {0}", key);

            //get all silos
            Dictionary<SiloAddress, SiloStatus> hosts = await SiloUtils.GetHosts(true);

            ISet<Task<IOrleansQueryResult<IIndexableGrain>>> queriesToSilos = GetResultQueries(hosts, key);

            //TODO: After fixing the problem with OrleansStream, this part is not needed anymore
            while (queriesToSilos.Count > 0)
            {
                // Identify the first task that completes.
                Task<IOrleansQueryResult<IIndexableGrain>> firstFinishedTask = await Task.WhenAny(queriesToSilos);

                // ***Remove the selected task from the list so that you don't
                // process it more than once.
                queriesToSilos.Remove(firstFinishedTask);

                // Await the completed task.
                IOrleansQueryResult<IIndexableGrain> partialResult = await firstFinishedTask;

                await result.OnNextBatchAsync(partialResult);
            }
            await result.OnCompletedAsync();
        }
    }
}
