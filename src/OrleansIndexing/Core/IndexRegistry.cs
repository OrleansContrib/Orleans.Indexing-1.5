using Orleans.Runtime;
using System;
using System.Collections.Generic;

namespace Orleans.Indexing
{
    public static class IndexRegistry
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
            IDictionary<string, Tuple<object, object, object>> indexes;
            if (!RuntimeClient.Current.Indexes.TryGetValue(iGrainType, out indexes))
            {
                return new Dictionary<string, Tuple<object, object, object>>();
            }
            return indexes;
        }

        /// <summary>
        /// Provides the index information for a given grain interface type.
        /// </summary>
        /// <typeparam name="T">The target grain interface type</typeparam>
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
        internal static IDictionary<string, Tuple<object, object, object>> GetIndexes<T>() where T : IIndexableGrain
        {
            return GetIndexes(typeof(T));
        }
    }
}
