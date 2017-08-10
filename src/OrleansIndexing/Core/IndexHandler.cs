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
        /// <summary>
        /// Provides the index information for a given grain interface type.
        /// </summary>
        /// <param name="iGrainType">The target grain interface type</param>
        /// <returns>the index information for the given grain type T.
        /// The index information is a dictionary from indexIDs defined
        /// on a grain interface to a triple. The triple consists of:
        /// 1) the index object (that implements IndexInterface,
        /// 2) the IndexMetaData object for this index, and
        /// 3) the IndexUpdateGenerator instance for this index.
        /// This triple is untyped, because IndexInterface, IndexMetaData
        /// and IndexUpdateGenerator types are not visible in this project.
        /// 
        /// This method returns an empty dictionary if the OrleansIndexing 
        /// project is not available.</returns>
        internal static IDictionary<string, Tuple<object, object, object>> GetIndexes(Type iGrainType)
        {
            return IndexRegistry.GetIndexes(iGrainType);
        }

        /// <summary>
        /// Provides the index information for a given grain interface type.
        /// </summary>
        /// <typeparam name="T">The target grain interface type</typeparam>
        /// <returns>the index information for the given grain type T.
        /// The index information is a dictionary from indexIDs defined
        /// on a grain interface to a triple. The triple consists of:
        /// 1) the index object that implements IndexInterface,
        /// 2) the IndexMetaData object for this index, and
        /// 3) the IndexUpdateGenerator instance for this index.
        /// This triple is untyped, because IndexInterface, IndexMetaData
        /// and IndexUpdateGenerator types are not visible in this project.
        /// 
        /// This method returns an empty dictionary if the OrleansIndexing 
        /// project is not available.</returns>
        internal static IDictionary<string, Tuple<object, object, object>> GetIndexes<T>() where T : IIndexableGrain
        {
            return IndexRegistry.GetIndexes<T>();
        }

        /// <summary>
        /// Retrieves the index object for a given indexed field on a indexed grain interface
        /// </summary>
        /// <param name="iGrainType">The target grain interface type</param>
        /// <param name="indexName">The target index name on the target grain interface type</param>
        /// <returns>the index object that implements IndexInterface</returns>
        internal static IndexInterface GetIndex(Type iGrainType, string indexName)
        {
            Tuple<object, object, object> index;
            if (GetIndexes(iGrainType).TryGetValue(indexName, out index))
            {
                return (IndexInterface)index.Item1;
            }
            else
            {
                //it should never happen that the indexes are not loaded if the
                //index is registered in the index registry
                throw new Exception(string.Format("Index \"{0}\" does not exist for {1}.", indexName, iGrainType));
                //}
            }
        }

        /// <summary>
        /// Retrieves the index object for a given indexed field on a indexed grain interface
        /// </summary>
        /// <typeparam name="T">The target grain interface type</typeparam>
        /// <param name="indexName">The target index name on the target grain interface type</param>
        /// <returns>the index object that implements IndexInterface</returns>
        internal static IndexInterface GetIndex<T>(string indexName) where T : IIndexableGrain
        {
            return GetIndex(typeof(T), indexName);
        }

        /// <summary>
        /// Retrieves the index object for a given indexed field on a indexed grain interface
        /// </summary>
        /// <typeparam name="K">key type of the target index</typeparam>
        /// <typeparam name="V">value type of the target index (i.e., grain interface type)</typeparam>
        /// <param name="indexName">The target index name on the target grain interface type</param>
        /// <returns>the index object that implements IndexInterface</returns>
        internal static IndexInterface<K,V> GetIndex<K,V>(string indexName) where V : IIndexableGrain
        {
            return (IndexInterface<K,V>)GetIndex(typeof(V), indexName);
        }
    }
}
