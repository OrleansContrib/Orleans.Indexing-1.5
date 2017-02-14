using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Runtime;
using K = System.Object;
using V = Orleans.Indexing.IIndexableGrain;
using Orleans.Providers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Orleans.Indexing
{
    /// <summary>
    /// A simple implementation of a single-grain in-memory hash-index
    /// 
    /// Generic SystemTargets are not supported yet, and that's why the
    /// implementation is non-generic.
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    [StorageProvider(ProviderName = Constants.MEMORY_STORAGE_PROVIDER_NAME)]
    [Reentrant]
    internal class AHashIndexPartitionedPerSiloBucketImpl/*<K, V>*/ : SystemTarget, AHashIndexPartitionedPerSiloBucket/*<K, V> where V : IIndexableGrain*/
    {
        private HashIndexBucketState<K, V> State;
        private readonly Logger logger;
        private readonly string _parentIndexName;
        
        public AHashIndexPartitionedPerSiloBucketImpl(string parentIndexName, GrainId grainId, SiloAddress silo) : base(grainId, silo)
        {
            State = new HashIndexBucketState<K, V>();
            State.IndexMap = new Dictionary<K, HashIndexSingleBucketEntry<V>>();
            State.IndexStatus = IndexStatus.Available;
            //State.IsUnique = false; //a per-silo index cannot check for uniqueness
            _parentIndexName = parentIndexName;

            logger = LogManager.GetLogger(string.Format("{0}.AHashIndexPartitionedPerSiloBucketImpl<{1},{2}>", parentIndexName, typeof(K), typeof(V)), LoggerType.Grain);
        }

        public async Task<bool> DirectApplyIndexUpdateBatch(Immutable<IDictionary<IIndexableGrain, IList<IMemberUpdate>>> iUpdates, bool isUnique, IndexMetaData idxMetaData, SiloAddress siloAddress = null)
        {
            if (logger.IsVerbose) logger.Verbose("Started calling DirectApplyIndexUpdateBatch with the following parameters: isUnique = {0}, siloAddress = {1}, iUpdates = {2}", isUnique, siloAddress, MemberUpdate.UpdatesToString(iUpdates.Value));

            IDictionary<IIndexableGrain, IList<IMemberUpdate>> updates = iUpdates.Value;
            Task[] updateTasks = new Task[updates.Count()];
            int i = 0;
            foreach (var kv in updates)
            {
                updateTasks[i] = DirectApplyIndexUpdates(kv.Key, kv.Value, isUnique, idxMetaData, siloAddress);
                ++i;
            }
            await Task.WhenAll(updateTasks);

            if (logger.IsVerbose) logger.Verbose("Finished calling DirectApplyIndexUpdateBatch with the following parameters: isUnique = {0}, siloAddress = {1}, iUpdates = {2}", isUnique, siloAddress, MemberUpdate.UpdatesToString(iUpdates.Value));

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task DirectApplyIndexUpdates(IIndexableGrain g, IList<IMemberUpdate> updates, bool isUniqueIndex, IndexMetaData idxMetaData, SiloAddress siloAddress)
        {
            foreach (IMemberUpdate updt in updates)
            {
                await DirectApplyIndexUpdate(g, updt, isUniqueIndex, idxMetaData, siloAddress);
            }
        }

        public Task<bool> DirectApplyIndexUpdate(IIndexableGrain g, Immutable<IMemberUpdate> iUpdate, bool isUniqueIndex, IndexMetaData idxMetaData, SiloAddress siloAddress)
        {
            return DirectApplyIndexUpdate(g, iUpdate.Value, isUniqueIndex, idxMetaData, siloAddress);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task<bool> DirectApplyIndexUpdate(IIndexableGrain g, IMemberUpdate updt, bool isUniqueIndex, IndexMetaData idxMetaData, SiloAddress siloAddress)
        {
            //the index can start processing update as soon as it becomes
            //visible to index handler and does not have to wait for any
            //further event regarding index builder, so it is not necessary
            //to have a Created state
            //if (State.IndexStatus == IndexStatus.Created) return true;

            //GrainFactory gFactory = InsideRuntimeClient.Current.InternalGrainFactory;

            V updatedGrain = g;//.AsReference<V>(gFactory);
            HashIndexBucketUtils.UpdateBucket(updatedGrain, updt, State, isUniqueIndex, idxMetaData);
            return Task.FromResult(true);
        }

        //public Task<bool> IsUnique()
        //{
        //    return Task.FromResult(State.IsUnique);
        //}

        public async Task Lookup(IOrleansQueryResultStream<V> result, K key)
        {
            if (logger.IsVerbose) logger.Verbose("Streamed index lookup called for key = {0}", key);

            if (!(State.IndexStatus == IndexStatus.Available))
            {
                var e = new Exception(string.Format("Index is not still available."));
                logger.Error((int)ErrorCode.IndexingIndexIsNotReadyYet, "Index is not still available.", e);
                throw e;
            }
            HashIndexSingleBucketEntry<V> entry;
            if (State.IndexMap.TryGetValue(key, out entry) && !entry.isTentative())
            {
                await result.OnNextBatchAsync(entry.Values);
                await result.OnCompletedAsync();
            }
            else
            {
                await result.OnCompletedAsync();
            }
        }

        //Task IndexInterface.Lookup(IOrleansQueryResultStream<IIndexableGrain> result, object key)
        //{
        //    return Lookup((IOrleansQueryResultStream<V>)result, (K)key);
        //}

        public Task<IOrleansQueryResult<V>> Lookup(K key)
        {
            if (logger.IsVerbose) logger.Verbose("Eager index lookup called for key = {0}", key);

            if (!(State.IndexStatus == IndexStatus.Available))
            {
                var e = new Exception(string.Format("Index is not still available."));
                logger.Error((int)ErrorCode.IndexingIndexIsNotReadyYet, "Index is not still available.", e);
                throw e;
            }
            HashIndexSingleBucketEntry<V> entry;
            if (State.IndexMap.TryGetValue(key, out entry) && !entry.isTentative())
            {
                return Task.FromResult((IOrleansQueryResult<V>)new OrleansQueryResult<V>(entry.Values));
            }
            else
            {
                return Task.FromResult((IOrleansQueryResult<V>)new OrleansQueryResult<V>(Enumerable.Empty<V>()));
            }
        }

        public Task<V> LookupUnique(K key)
        {
            if (!(State.IndexStatus == IndexStatus.Available))
            {
                var e = new Exception(string.Format("Index is not still available."));
                logger.Error((int)ErrorCode.IndexingIndexIsNotReadyYet, e.Message, e);
                throw e;
            }
            HashIndexSingleBucketEntry<V> entry;
            if (State.IndexMap.TryGetValue(key, out entry) && !entry.isTentative())
            {
                if (entry.Values.Count() == 1)
                {
                    return Task.FromResult(entry.Values.GetEnumerator().Current);
                }
                else
                {
                    var e = new Exception(string.Format("There are {0} values for the unique lookup key \"{1}\" does not exist on index \"{2}->{3}\".", entry.Values.Count(), key, _parentIndexName, IndexUtils.GetIndexNameFromIndexGrain(this)));
                    logger.Error((int)ErrorCode.IndexingIndexIsNotReadyYet, e.Message, e);
                    throw e;
                }
            }
            else
            {
                var e = new Exception(string.Format("The lookup key \"{0}\" does not exist on index \"{1}->{2}\".", key, _parentIndexName, IndexUtils.GetIndexNameFromIndexGrain(this)));
                logger.Error((int)ErrorCode.IndexingIndexIsNotReadyYet, e.Message, e);
                throw e;
            }
        }

        public Task Dispose()
        {
            State.IndexStatus = IndexStatus.Disposed;
            State.IndexMap.Clear();
            Runtime.Silo.CurrentSilo.UnregisterSystemTarget(this);
            return TaskDone.Done;
        }

        private IIndexBuilder<V> GetIndexBuilder(IGrainFactory gf)
        {
            return gf.GetGrain<IIndexBuilder<V>>(IndexUtils.GetIndexGrainID(typeof(V), IndexUtils.GetIndexNameFromIndexGrain(this)));
        }

        public Task<bool> IsAvailable()
        {
            return Task.FromResult(State.IndexStatus == IndexStatus.Available);
        }
    }
}
