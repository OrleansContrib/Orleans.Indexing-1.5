using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime;
using System.Linq.Expressions;
using System.Reflection;
using Orleans.Streams;

namespace Orleans.Indexing
{
    /// <summary>
    /// A utility class for the index operations
    /// </summary>
    public static class IndexFactory
    {
        /// <summary>
        /// This method queries the active grains for the given
        /// grain interface and the filter expression. The filter
        /// expression should contain an indexed field.
        /// </summary>
        /// <typeparam name="TIGrain">the given grain interface
        /// type to query over its active instances</typeparam>
        /// <param name="gf">the grain factory instance</param>
        /// <param name="filterExpr">the filter expression of the query</param>
        /// <param name="queryResultObserver">the observer object to be called
        /// on every grain found for the query</param>
        /// <returns>the result of the query</returns>
        public static Task GetActiveGrains<TIGrain, TProperties>(this IGrainFactory gf, Expression<Func<TProperties, bool>> filterExpr, IAsyncBatchObserver<TIGrain> queryResultObserver) where TIGrain : IIndexableGrain
        {
            return GrainClient.GrainFactory.GetActiveGrains<TIGrain, TProperties>()
                                           .Where(filterExpr)
                                           .ObserveResults(queryResultObserver);
        }

        /// <summary>
        /// This method queries the active grains for the given
        /// grain interface and the filter expression. The filter
        /// expression should contain an indexed field.
        /// </summary>
        /// <typeparam name="TIGrain">the given grain interface
        /// type to query over its active instances</typeparam>
        /// <param name="gf">the grain factory instance</param>
        /// <param name="streamProvider">the stream provider for the query results</param>
        /// <returns>the query to lookup all active grains of a given type</returns>
        /// <param name="filterExpr">the filter expression of the query</param>
        /// <param name="queryResultObserver">the observer object to be called
        /// on every grain found for the query</param>
        /// <returns>the result of the query</returns>
        public static Task GetActiveGrains<TIGrain, TProperties>(this IGrainFactory gf, IStreamProvider streamProvider, Expression<Func<TProperties, bool>> filterExpr, IAsyncBatchObserver<TIGrain> queryResultObserver) where TIGrain : IIndexableGrain
        {
            return GrainClient.GrainFactory.GetActiveGrains<TIGrain, TProperties>(streamProvider)
                                           .Where(filterExpr)
                                           .ObserveResults(queryResultObserver);
        }

        /// <summary>
        /// This method queries the active grains for the given
        /// grain interface.
        /// </summary>
        /// <typeparam name="TIGrain">the given grain interface
        /// type to query over its active instances</typeparam>
        /// <param name="gf">the grain factory instance</param>
        /// <returns>the query to lookup all active grains of a given type</returns>
        public static IOrleansQueryable<TIGrain, TProperty> GetActiveGrains<TIGrain, TProperty>(this IGrainFactory gf) where TIGrain : IIndexableGrain
        {
            return GetActiveGrains<TIGrain, TProperty>(gf, GrainClient.GetStreamProvider(Constants.INDEXING_STREAM_PROVIDER_NAME));
        }

        /// <summary>
        /// This method queries the active grains for the given
        /// grain interface.
        /// </summary>
        /// <typeparam name="TIGrain">the given grain interface
        /// type to query over its active instances</typeparam>
        /// <param name="gf">the grain factory instance</param>
        /// <param name="streamProvider">the stream provider for the query results</param>
        /// <returns>the query to lookup all active grains of a given type</returns>
        public static IOrleansQueryable<TIGrain, TProperty> GetActiveGrains<TIGrain, TProperty>(this IGrainFactory gf, IStreamProvider streamProvider) where TIGrain : IIndexableGrain
        {
            return new QueryActiveGrainsNode<TIGrain, TProperty>(gf, streamProvider);
        }

        /// <summary>
        /// Gets an IndexInterface<K,V> given its name
        /// </summary>
        /// <typeparam name="K">key type of the index</typeparam>
        /// <typeparam name="V">value type of the index, which is
        /// the grain being indexed</typeparam>
        /// <param name="indexName">the name of the index, which
        /// is the identifier of the index</param>
        /// <returns>the IndexInterface<K,V> with the specified name</returns>
        public static IndexInterface<K, V> GetIndex<K, V>(this IGrainFactory gf, string indexName) where V : IIndexableGrain
        {
            return IndexHandler.GetIndex<K,V>(indexName);
        }

        /// <summary>
        /// Gets an IndexInterface given its name and grain interface type
        /// </summary>
        /// <param name="indexName">the name of the index, which
        /// is the identifier of the index<</param>
        /// <param name="iGrainType">the grain interface type
        /// that is being indexed</param>
        /// <returns>the IndexInterface with the specified name on the
        /// given grain interface type</returns>
        internal static IndexInterface GetIndex(this IGrainFactory gf, string indexName, Type iGrainType)
        {
            return IndexHandler.GetIndex(iGrainType, indexName);
        }

        /// <summary>
        /// Creates an index grain, given its type and
        /// the type of its IndexUpdateGenerator.
        /// 
        /// The created index grain is not registered and will not do anything 
        /// unless it is registered by calling IndexFactory.RegisterIndex.
        /// </summary>
        /// <typeparam name="IIdxType">the type of grain interface
        /// of the index</typeparam>
        /// <typeparam name="IndexUpdateGenType">the type of
        /// IndexUpdateGenerator of the index</typeparam>
        /// <param name="indexName">the name of the index, which
        /// is the identifier of the index</param>
        /// <returns>the created index grain</returns>
        //internal static IIdxType CreateIndexGrain<IIdxType>(this IGrainFactory gf, string indexName) where IIdxType : IndexInterface
        //{
        //    Type idxType = typeof(IIdxType);
        //    Type iIndexType = idxType.GetGenericType(typeof(IndexInterface<,>));
        //    if (iIndexType != null)
        //    {
        //        Type[] indexTypeArgs = iIndexType.GetGenericArguments();
        //        //Type keyType = indexTypeArgs[0];
        //        Type grainType = indexTypeArgs[1];

        //        IIdxType indexGrain = gf.GetGrain<IIdxType>(IndexUtils.GetIndexGrainID(grainType, indexName));
        //        return indexGrain;
        //    }
        //    else
        //    {
        //        throw new NotSupportedException(string.Format("Adding an index that does not implement IndexInterface<K,V> is not supported yet. Your requested index ({0}) is invalid.", idxType.ToString()));
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gf"></param>
        /// <param name="idxType"></param>
        /// <param name="indexName"></param>
        /// <returns></returns>
        internal static Tuple<object, object, object> CreateIndex(this IGrainFactory gf, Type idxType, string indexName, bool isUniqueIndex, bool isEager, int maxEntriesPerBucket, PropertyInfo indexedProperty)
        {
            Type iIndexType = idxType.GetGenericType(typeof(IndexInterface<,>));
            if (iIndexType != null)
            {
                Type[] indexTypeArgs = iIndexType.GetGenericArguments();
                Type keyType = indexTypeArgs[0];
                Type grainType = indexTypeArgs[1];

                IndexInterface index;
                if (typeof(IGrain).IsAssignableFrom(idxType))
                {
                    index = (IndexInterface)gf.GetGrain(IndexUtils.GetIndexGrainID(grainType, indexName), idxType, idxType);

                    Type idxImplType = TypeUtils.ResolveType(TypeCodeMapper.GetImplementation(idxType).GrainClass);

                    if(idxImplType.IsGenericTypeDefinition)
                        idxImplType = idxImplType.MakeGenericType(iIndexType.GetGenericArguments());

                    MethodInfo initPerSilo;
                    if ((initPerSilo = idxImplType.GetMethod("InitPerSilo", BindingFlags.Static | BindingFlags.Public)) != null)
                    {
                        var initPerSiloMethod = (Action<Silo, string, bool>)Delegate.CreateDelegate(
                                                typeof(Action<Silo, string, bool>),
                                                initPerSilo);
                        initPerSiloMethod(Silo.CurrentSilo, indexName, isUniqueIndex);
                    }
                }
                else if (idxType.IsClass)
                {
                    index = (IndexInterface)Activator.CreateInstance(idxType, indexName, isUniqueIndex);
                }
                else
                {
                    throw new Exception(string.Format("{0} is neither a grain nor a class. Index \"{1}\" cannot be created.", idxType, indexName));
                }

                return Tuple.Create((object)index, (object)new IndexMetaData(idxType, isUniqueIndex, isEager, maxEntriesPerBucket), (object)CreateIndexUpdateGenFromProperty(indexedProperty));
            }
            else
            {
                throw new NotSupportedException(string.Format("Adding an index that does not implement IndexInterface<K,V> is not supported yet. Your requested index ({0}) is invalid.", idxType.ToString()));
            }
        }

        private static IIndexUpdateGenerator CreateIndexUpdateGenFromProperty(PropertyInfo indexedProperty)
        {
            return new IndexUpdateGenerator(indexedProperty);
        }

        /// <summary>
        /// Registers the given index with the given name
        /// into the Orleans Indexing runtime.
        /// </summary>
        /// <typeparam name="IIdxType">the type of the index to
        /// be registered</typeparam>
        /// <param name="indexName">the name of the index, which
        /// is the identifier of the index</param>
        /// <param name="index">the index grain to be registered</param>
        /// <returns>whether the registration of the index was
        /// successful or not.</returns>
        //public static async Task<bool> RegisterIndex<IIdxType, IndexUpdateGenType>(this IGrainFactory gf, string indexName, IIdxType index) where IIdxType : IndexInterface where IndexUpdateGenType : IIndexUpdateGenerator, new()
        //{
        //    Type idxType = index.GetType();
        //    Type iIndexType = idxType.GetGenericType(typeof(IndexInterface<,>));
        //    if (iIndexType != null)
        //    {
        //        Type[] indexTypeArgs = iIndexType.GetGenericArguments();
        //        //Type keyType = indexTypeArgs[0];
        //        Type iGrainType = indexTypeArgs[1];

        //        //string indexName = await index.GetIndexName();
        //        bool isRegistered = await IndexRegistry.RegisterIndex(iGrainType, indexName, index, new IndexMetaData(typeof(IIdxType), typeof(IndexUpdateGenType)));
        //        if (isRegistered)
        //        {
        //            Type indexBuilderType = typeof(IIndexBuilder<>).MakeGenericType(new Type[] { iGrainType });
        //            IIndexBuilder indexBuilder = gf.GetGrain<IIndexBuilder<IIndexableGrain>>(IndexUtils.GetIndexGrainID(iGrainType, indexName), indexBuilderType);
        //            var _ = indexBuilder.BuildIndex(indexName, index, new IndexUpdateGenType()); //builds the index on its own without coming back here
        //        }
        //        return isRegistered;
        //    }
        //    else
        //    {
        //        throw new NotSupportedException(string.Format("Registering indexes that do not implement IndexInterface<K,V> is not supported yet. Your requested index ({0}) is invalid.", idxType.ToString()));
        //    }
        //}

        /// <summary>
        /// A call to CreateIndexGrain followed by a call to RegisterIndex.
        /// </summary>
        /// <typeparam name="IIdxType">the type of the index to
        /// be registered</typeparam>
        /// <typeparam name="IndexUpdateGenType">the type of
        /// IndexUpdateGenerator of the index</typeparam>
        /// <param name="indexName">the name of the index, which
        /// is the identifier of the index</param>
        /// <returns>whether the creation and registration of the
        /// index was successful or not.</returns>
        //public static Task<bool> CreateAndRegisterIndex<IIdxType, IndexUpdateGenType>(this IGrainFactory gf, string indexName) where IIdxType : IndexInterface where IndexUpdateGenType : IIndexUpdateGenerator, new()
        //{
        //    IIdxType index = CreateIndexGrain<IIdxType>(gf, indexName);
        //    return RegisterIndex<IIdxType, IndexUpdateGenType>(gf, indexName, index);
        //}

        /// <summary>
        /// Drops all the indexes defined for a given grain interface.
        /// </summary>
        /// <typeparam name="IGrainType">the given grain interface</typeparam>
        //public static async Task DropAllIndexes<IGrainType>(this IGrainFactory gf) where IGrainType : IIndexableGrain
        //{
        //    await IndexRegistry.DropAllIndexes<IGrainType>();
        //}

        /// <summary>
        /// Drops an index defined for a given grain interface provided its name.
        /// </summary>
        /// <typeparam name="IGrainType">the given grain interface</typeparam>
        /// <param name="indexName">the name of the index</param>
        //public static async Task DropIndex<IGrainType>(this IGrainFactory gf, string indexName) where IGrainType : IIndexableGrain
        //{
        //    await IndexRegistry.DropIndex<IGrainType>(indexName);
        //}
    }
}
