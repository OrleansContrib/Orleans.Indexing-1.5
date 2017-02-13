using System;
using System.Collections.Generic;

namespace Orleans.Indexing
{
    /// <summary>
    /// IndexHandler is responsible for updating the indexes defined
    /// for a grain interface type. It  also communicates with the grain
    /// instances by telling them about the list of available indexes.
    /// 
    /// The fact that IndexHandler is a StatelessWorker makes it
    /// very scalable, but at the same time should stay in sync
    /// with index registry to be aware of the available indexes.
    /// </summary>
    internal static class IndexHandler
    {
        //internal static Task<bool> ApplyIndexUpdates<T>(IIndexableGrain updatedGrain, Immutable<IDictionary<string, IMemberUpdate>> iUpdates) where T : IIndexableGrain
        //{
        //    return ApplyIndexUpdates(typeof(T), updatedGrain, iUpdates);
        //}

        internal static IDictionary<string, Tuple<object, object, object>> GetIndexes(Type iGrainType)
        {
            return IndexRegistry.GetIndexes(iGrainType);
        }

        internal static IDictionary<string, Tuple<object, object, object>> GetIndexes<T>() where T : IIndexableGrain
        {
            return IndexRegistry.GetIndexes<T>();
        }

        internal static IndexInterface GetIndex(Type iGrainType, string indexName)
        {
            Tuple<object, object, object> index;
            if (GetIndexes(iGrainType).TryGetValue(indexName, out index))
            {
                return (IndexInterface)index.Item1;
            }
            else
            {
                //this part of code is commented out, because it should
                //never happen that the indexes are not loaded, if the
                //index is registered in the index registry
                //await ReloadIndexes();
                //if (_indexes.Value.TryGetValue(indexName, out index))
                //{
                //    return Task.FromResult(index.Item1);
                //}
                //else
                //{
                throw new Exception(string.Format("Index \"{0}\" does not exist for {1}.", indexName, iGrainType));
                //}
            }
        }

        internal static IndexInterface GetIndex<T>(string indexName) where T : IIndexableGrain
        {
            return GetIndex(typeof(T), indexName);
        }

        internal static IndexInterface<K,V> GetIndex<K,V>(string indexName) where V : IIndexableGrain
        {
            return (IndexInterface<K,V>)GetIndex(typeof(V), indexName);
        }
    }
}
