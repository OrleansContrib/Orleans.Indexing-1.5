using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace Orleans.Indexing
{
    /// <summary>
    /// A simple implementation of a partitioned in-memory hash-index
    /// </summary>
    /// <typeparam name="K">type of hash-index key</typeparam>
    /// <typeparam name="V">type of grain that is being indexed</typeparam>
    public abstract class HashIndexPartitionedPerKey<K, V, BucketT> : HashIndexInterface<K, V> where V : class, IIndexableGrain where BucketT : HashIndexPartitionedPerKeyBucketInterface<K, V>, IGrainWithStringKey
    {
        private string _indexName;
        //private bool _isUnique;

        private static readonly Logger logger = LogManager.GetLogger(string.Format("HashIndexPartitionedPerKey<{0},{1}>", typeof(K).Name, typeof(V).Name), LoggerType.Grain);

        public HashIndexPartitionedPerKey(string indexName, bool isUniqueIndex)
        {
            _indexName = indexName;
            //_isUnique = isUniqueIndex;
        }

        public async Task<bool> DirectApplyIndexUpdateBatch(Immutable<IDictionary<IIndexableGrain, IList<IMemberUpdate>>> iUpdates, bool isUnique, IndexMetaData idxMetaData, SiloAddress siloAddress = null)
        {
            if(logger.IsVerbose) logger.Verbose("Started calling DirectApplyIndexUpdateBatch with the following parameters: isUnique = {0}, siloAddress = {1}, iUpdates = {2}", isUnique, siloAddress, MemberUpdate.UpdatesToString(iUpdates.Value));
            
            IDictionary<IIndexableGrain, IList<IMemberUpdate>> updates = iUpdates.Value;
            IDictionary<int, IDictionary<IIndexableGrain, IList<IMemberUpdate>>> bucketUpdates = new Dictionary<int, IDictionary<IIndexableGrain, IList<IMemberUpdate>>>();
            foreach (var kv in updates)
            {
                IIndexableGrain g = kv.Key;
                IList<IMemberUpdate> gUpdates = kv.Value;
                foreach(IMemberUpdate update in gUpdates)
                {
                    IndexOperationType opType = update.GetOperationType();
                    if (opType == IndexOperationType.Update)
                    {
                        int befImgHash = update.GetBeforeImage().GetHashCode();
                        int aftImgHash = update.GetAfterImage().GetHashCode();

                        if (befImgHash == aftImgHash)
                        {
                            AddUpdateToBucket(bucketUpdates, g, befImgHash, update);
                        }
                        else
                        {
                            AddUpdateToBucket(bucketUpdates, g, befImgHash, new MemberUpdateOverridenOperation(update, IndexOperationType.Delete));
                            AddUpdateToBucket(bucketUpdates, g, aftImgHash, new MemberUpdateOverridenOperation(update, IndexOperationType.Insert));
                        }
                    }
                    else if (opType == IndexOperationType.Insert)
                    {
                        int aftImgHash = update.GetAfterImage().GetHashCode();
                        AddUpdateToBucket(bucketUpdates, g, aftImgHash, update);
                    }
                    else if (opType == IndexOperationType.Delete)
                    {
                        int befImgHash = update.GetBeforeImage().GetHashCode();
                        AddUpdateToBucket(bucketUpdates, g, befImgHash, update);
                    }
                }
            }

            List<Task> updateTasks = new List<Task>();
            int i = 0;
            foreach (var kv in bucketUpdates)
            {
                BucketT bucket = InsideRuntimeClient.Current.InternalGrainFactory.GetGrain<BucketT>(
                    IndexUtils.GetIndexGrainID(typeof(V), _indexName) + "_" + kv.Key
                );
                updateTasks.Add( bucket.DirectApplyIndexUpdateBatch(kv.Value.AsImmutable(), isUnique, idxMetaData, siloAddress) );
                ++i;
            }
            await Task.WhenAll(updateTasks);
            if (logger.IsVerbose) logger.Verbose("Finished calling DirectApplyIndexUpdateBatch with the following parameters: isUnique = {0}, siloAddress = {1}, iUpdates = {2}", isUnique, siloAddress, MemberUpdate.UpdatesToString(iUpdates.Value));
            
            return true;
        }

        /// <summary>
        /// Adds an grain update to the bucketUpdates dictionary
        /// </summary>
        /// <param name="bucketUpdates">the bucketUpdates dictionary</param>
        /// <param name="g">target grain</param>
        /// <param name="bucket">the bucket index</param>
        /// <param name="update">the update to be added</param>
        private void AddUpdateToBucket(IDictionary<int, IDictionary<IIndexableGrain, IList<IMemberUpdate>>> bucketUpdates, IIndexableGrain g, int bucket, IMemberUpdate update)
        {
            IDictionary<IIndexableGrain, IList<IMemberUpdate>> tmpBucketUpdatesMap;
            IList<IMemberUpdate> tmpUpdateList;

            if (bucketUpdates.TryGetValue(bucket, out tmpBucketUpdatesMap))
            {
                if (!tmpBucketUpdatesMap.TryGetValue(g, out tmpUpdateList))
                {
                    tmpUpdateList = new List<IMemberUpdate>(new[] { update });
                    tmpBucketUpdatesMap.Add(g, tmpUpdateList);
                }
                else
                {
                    tmpUpdateList.Add(update);
                }
            }
            else
            {
                tmpBucketUpdatesMap = new Dictionary<IIndexableGrain, IList<IMemberUpdate>>();
                tmpBucketUpdatesMap.Add(g, new List<IMemberUpdate>(new[] { update }));
                bucketUpdates.Add(bucket, tmpBucketUpdatesMap);
            }
        }

        public async Task<bool> DirectApplyIndexUpdate(IIndexableGrain g, Immutable<IMemberUpdate> iUpdate, bool isUniqueIndex, IndexMetaData idxMetaData, SiloAddress siloAddress)
        {
            IMemberUpdate update = iUpdate.Value;
            IndexOperationType opType = update.GetOperationType();
            if (opType == IndexOperationType.Update)
            {
                int befImgHash = update.GetBeforeImage().GetHashCode();
                int aftImgHash = update.GetAfterImage().GetHashCode();
                BucketT befImgBucket = InsideRuntimeClient.Current.InternalGrainFactory.GetGrain<BucketT>(
                    IndexUtils.GetIndexGrainID(typeof(V), _indexName) + "_" + befImgHash
                );
                if (befImgHash == aftImgHash)
                {
                    return await befImgBucket.DirectApplyIndexUpdate(g, iUpdate, isUniqueIndex, idxMetaData);
                }
                else
                {
                    BucketT aftImgBucket = InsideRuntimeClient.Current.InternalGrainFactory.GetGrain<BucketT>(
                        IndexUtils.GetIndexGrainID(typeof(V), _indexName) + "_" + befImgHash
                    );
                    var befTask = befImgBucket.DirectApplyIndexUpdate(g, new MemberUpdateOverridenOperation(iUpdate.Value, IndexOperationType.Delete).AsImmutable<IMemberUpdate>(), isUniqueIndex, idxMetaData);
                    var aftTask = aftImgBucket.DirectApplyIndexUpdate(g, new MemberUpdateOverridenOperation(iUpdate.Value, IndexOperationType.Insert).AsImmutable<IMemberUpdate>(), isUniqueIndex, idxMetaData);
                    bool[] results = await Task.WhenAll(befTask, aftTask);
                    return results[0] && results[1];
                }
            }
            else if(opType == IndexOperationType.Insert)
            {
                int aftImgHash = update.GetAfterImage().GetHashCode();
                BucketT aftImgBucket = InsideRuntimeClient.Current.InternalGrainFactory.GetGrain<BucketT>(
                    IndexUtils.GetIndexGrainID(typeof(V), _indexName) + "_" + aftImgHash
                );
                return await aftImgBucket.DirectApplyIndexUpdate(g, iUpdate, isUniqueIndex, idxMetaData);
            }
            else if(opType == IndexOperationType.Delete)
            {
                int befImgHash = update.GetBeforeImage().GetHashCode();
                BucketT befImgBucket = InsideRuntimeClient.Current.InternalGrainFactory.GetGrain<BucketT>(
                    IndexUtils.GetIndexGrainID(typeof(V), _indexName) + "_" + befImgHash
                );
                return await befImgBucket.DirectApplyIndexUpdate(g, iUpdate, isUniqueIndex, idxMetaData);
            }
            return true;
        }

        public Task Lookup(IOrleansQueryResultStream<V> result, K key)
        {
            if (logger.IsVerbose) logger.Verbose("Streamed index lookup called for key = {0}", key);
            BucketT targetBucket = RuntimeClient.Current.InternalGrainFactory.GetGrain<BucketT>(
                IndexUtils.GetIndexGrainID(typeof(V), _indexName) + "_" + key.GetHashCode()
            );
            return targetBucket.Lookup(result, key);
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

        public Task Dispose()
        {
            //right now, we cannot do anything.
            //we need to know the list of buckets
            return TaskDone.Done;
        }

        public Task<bool> IsAvailable()
        {
            return Task.FromResult(true);
        }

        Task IndexInterface.Lookup(IOrleansQueryResultStream<IIndexableGrain> result, object key)
        {
            return Lookup(result.Cast<V>(), (K)key);
        }

        public Task<IOrleansQueryResult<V>> Lookup(K key)
        {
            if (logger.IsVerbose) logger.Verbose("Eager index lookup called for key = {0}", key);
            BucketT targetBucket = RuntimeClient.Current.InternalGrainFactory.GetGrain<BucketT>(
                   IndexUtils.GetIndexGrainID(typeof(V), _indexName) + "_" + key.GetHashCode()
               );
            return targetBucket.Lookup(key);
        }

        async Task<IOrleansQueryResult<IIndexableGrain>> IndexInterface.Lookup(object key)
        {
            return await Lookup((K)key);
        }
    }
}
