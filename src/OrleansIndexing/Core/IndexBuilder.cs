using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    [Reentrant]
    public class IndexBuilder<T> : Grain, IIndexBuilder<T> where T : IIndexableGrain
    {

        private IndexBuilderStatus _status;

        private ISet<T> _tombstones;

        private ISet<T> _failedToIndex;

        private Logger Log { get { return GetLogger(); } }

        public override Task OnActivateAsync()
        {
            _status = IndexBuilderStatus.Created;
            return base.OnActivateAsync();
        }

        public async Task BuildIndex(string indexName, IndexInterface index, IndexMetaData indexMetaData, IIndexUpdateGenerator iUpdateGen)
        {
            if (_status != IndexBuilderStatus.InProgress)
            {
                _status = IndexBuilderStatus.InProgress;
                try
                {
                    _tombstones = new HashSet<T>();
                    _failedToIndex = new HashSet<T>();

                    //getting the current list of active silos
                    var silos = (await SiloUtils.GetHosts(true)).Keys;

                    //for each silo, the list of active grains of type T are found,
                    //and then each one is added to the index
                    foreach (SiloAddress silo in silos)
                    {
                        IEnumerable<T> activeGrainsSnapshot = await ActiveGrainScanner.GetActiveGrains<T>(GrainFactory, silo);
                        foreach (T iGrain in activeGrainsSnapshot)
                        {
                            await AddGrainToIndex(iGrain, index, indexMetaData, iUpdateGen);
                        }
                    }

                    //if we failed to index some grains, an error is logged and
                    //we try again to fix it
                    if (_failedToIndex.Count() != 0)
                    {
                        Log.Warn((int)ErrorCode.IndexingIndexBuilderFailedToBuildIndex, 
                            string.Format("Failed to index the following grains while building \"{0}\" index for {1}: {3}. Trying again.",
                            indexName, typeof(T), _failedToIndex));
                        IEnumerable<T> activeGrainsSnapshot = _failedToIndex;
                        _failedToIndex = new HashSet<T>();
                        foreach (T iGrain in activeGrainsSnapshot)
                        {
                            await AddGrainToIndex(iGrain, index, indexMetaData, iUpdateGen);
                        }
                        //if it failed again, writing to log is the only option left for us
                        //should we try again?
                        if (_failedToIndex.Count() != 0)
                        {
                            Log.Warn((int)ErrorCode.IndexingIndexBuilderFailedToBuildIndexAgain,
                                string.Format("Failed to index the following grains while building \"{0}\" index for {1}: {3}",
                                indexName, typeof(T), _failedToIndex));
                        }
                    }

                    _status = IndexBuilderStatus.Done;
                }
                catch (Exception e)
                {
                    //something went wrong, so we write a log record and try again
                    Log.Warn((int)ErrorCode.IndexingIndexBuilderFailedToBuildIndexAgain,
                        string.Format("Failed to build \"{0}\" index for {1}. Trying again.",
                        indexName, typeof(T)), e);

                    _status = IndexBuilderStatus.Created;
                    await BuildIndex(indexName, index, indexMetaData, iUpdateGen);
                }
            }
        }

        private async Task AddGrainToIndex(T iGrain, IndexInterface index, IndexMetaData indexMetaData, IIndexUpdateGenerator iUpdateGen)
        {
            if (!_tombstones.Contains(iGrain))
            {
                object grainImage = await iGrain.ExtractIndexImage(iUpdateGen);
                //Add it to the index
                if (await index.ApplyIndexUpdate(iGrain, ((IMemberUpdate)new IndexBuilderMemberUpdate(null, grainImage)).AsImmutable(), indexMetaData.IsUniqueIndex(), indexMetaData))
                {
                    //Check again for it in the list of tombstones
                    //It might be added to tombstones while being added
                    //to the index
                    if (_tombstones.Contains(iGrain))
                    {
                        //then delete it from the index
                        await index.ApplyIndexUpdate(iGrain, ((IMemberUpdate)new IndexBuilderMemberUpdate(grainImage, null)).AsImmutable(), indexMetaData.IsUniqueIndex(), indexMetaData);
                    }
                }
                else
                {
                    //gather the failed ones
                    _failedToIndex.Add(iGrain);
                }
            }
        }

        public Task<bool> AddTombstone(T removedGrain)
        {
            _tombstones.Add(removedGrain);
            return Task.FromResult(_status == IndexBuilderStatus.Done);
        }

        Task<bool> IIndexBuilder.AddTombstone(IIndexableGrain removedGrain)
        {
            return AddTombstone(removedGrain.AsReference<T>(GrainFactory));
        }

        public Task<IndexBuilderStatus> GetStatus()
        {
            return Task.FromResult(_status);
        }

        public Task<bool> IsDone()
        {
            return Task.FromResult(_status == IndexBuilderStatus.Done);
        }
    }

    public class IndexBuilderMemberUpdate : MemberUpdate
    {
        public IndexBuilderMemberUpdate(object befImg, object aftImg) : base(befImg, aftImg)
        {
        }
    }
}
