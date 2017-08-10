using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Orleans.CodeGeneration;
using Orleans.Serialization;
using Orleans.LogConsistency;
using Orleans.Providers;
using Orleans.Runtime.Configuration;

namespace Orleans.Runtime
{
    internal class SiloAssemblyLoader
    {
        private readonly List<string> excludedGrains;
        private readonly LoggerImpl logger = LogManager.GetLogger("AssemblyLoader.Silo");
        private List<string> discoveredAssemblyLocations;
        private Dictionary<string, SearchOption> directories;

        public SiloAssemblyLoader(NodeConfiguration nodeConfig)
            : this(nodeConfig.AdditionalAssemblyDirectories, nodeConfig.ExcludedGrainTypes)
        {
        }

        public SiloAssemblyLoader(IDictionary<string, SearchOption> additionalDirectories, IEnumerable<string> excludedGrains = null)
        {
            this.excludedGrains = excludedGrains != null
                ? new List<string>(excludedGrains)
                : new List<string>();
            var exeRoot = Path.GetDirectoryName(typeof(SiloAssemblyLoader).GetTypeInfo().Assembly.Location);
            var appRoot = Path.Combine(exeRoot, "Applications");
            var cwd = Directory.GetCurrentDirectory();

            directories = new Dictionary<string, SearchOption>
                    {
                        { exeRoot, SearchOption.TopDirectoryOnly },
                        { appRoot, SearchOption.AllDirectories }
                    };

            foreach (var kvp in additionalDirectories)
            {
                // Make sure the path is clean (get rid of ..\'s)
                directories[new DirectoryInfo(kvp.Key).FullName] = kvp.Value;
            }


            if (!directories.ContainsKey(cwd))
            {
                directories.Add(cwd, SearchOption.TopDirectoryOnly);
            }

            LoadApplicationAssemblies();
        }

        private void LoadApplicationAssemblies()
        {
#if !NETSTANDARD_TODO
            AssemblyLoaderPathNameCriterion[] excludeCriteria =
                {
                    AssemblyLoaderCriteria.ExcludeResourceAssemblies,
                    AssemblyLoaderCriteria.ExcludeSystemBinaries()
                };
            AssemblyLoaderReflectionCriterion[] loadCriteria =
                {
                    AssemblyLoaderReflectionCriterion.NewCriterion(
                        TypeUtils.IsConcreteGrainClass,
                        "Assembly does not contain any acceptable grain types."),
                    AssemblyLoaderCriteria.LoadTypesAssignableFrom(
                        typeof(IProvider))
                };

            discoveredAssemblyLocations = AssemblyLoader.LoadAssemblies(directories, excludeCriteria, loadCriteria, logger);
#endif
        }

        public IDictionary<string, GrainTypeData> GetGrainClassTypes(bool strict)
        {
            var result = new Dictionary<string, GrainTypeData>();
            Type[] grainTypes = strict
                ? TypeUtils.GetTypes(TypeUtils.IsConcreteGrainClass, logger).ToArray()
                : TypeUtils.GetTypes(discoveredAssemblyLocations, TypeUtils.IsConcreteGrainClass, logger).ToArray();

            foreach (var grainType in grainTypes)
            {
                var className = TypeUtils.GetFullName(grainType);
                if (excludedGrains.Contains(className))
                    continue;

                if (result.ContainsKey(className))
                    throw new InvalidOperationException(
                        string.Format("Precondition violated: GetLoadedGrainTypes should not return a duplicate type ({0})", className));

                Type grainStateType = null;

                // check if grainType derives from Grai<nT> where T is a concrete class

                var parentType = grainType.GetTypeInfo().BaseType;
                while (parentType != typeof(Grain) && parentType != typeof(object))
                {
                    TypeInfo parentTypeInfo = parentType.GetTypeInfo();
                    if (parentTypeInfo.IsGenericType)
                    {
                        var definition = parentTypeInfo.GetGenericTypeDefinition();
                        if (definition == typeof(Grain<>) || definition == typeof(LogConsistentGrainBase<>))
                        {
                            var stateArg = parentType.GetGenericArguments()[0];
                            if (stateArg.GetTypeInfo().IsClass || stateArg.GetTypeInfo().IsValueType)
                            {
                                grainStateType = stateArg;
                                break;
                            }
                        }
                    }

                    parentType = parentTypeInfo.BaseType;
                }

                GrainTypeData typeData = GetTypeData(grainType, grainStateType);
                result.Add(className, typeData);
            }

            LogGrainTypesFound(logger, result);
            return result;
        }

        public static string OrleansIndexingAssembly = "OrleansIndexing";
        public static string AssemblySeparator = ", ";

        //private static Type iIndexableGrainType;
        private static Type genericIIndexableGrainType;
        private static Type genericFaultTolerantIndexableGrainType;
        private static Type indexAttributeType;
        private static PropertyInfo indexTypeProperty;
        private static Type indexFactoryType;
        private static Func<IGrainFactory, Type, string, bool, bool, int, PropertyInfo, Tuple<object, object, object>> createIndexMethod;
        private static Action<Type, Type> registerIndexWorkflowQueuesMethod;
        private static PropertyInfo isEagerProperty;
        private static PropertyInfo isUniqueProperty;
        private static PropertyInfo maxEntriesPerBucketProperty;
        private static Type totalIndexType;

        /// <summary>
        /// This method crawls the assemblies and looks for the index
        /// definitions (determined by extending IIndexable{TProperties}
        /// interface and adding annotations to properties in TProperties).
        /// 
        /// In order to avoid having any dependency on OrleansIndexing
        /// project, all the required types are loaded via reflection.
        /// </summary>
        /// <param name="strict">determines the lookup strategy for
        /// looking into the assemblies</param>
        /// <returns>A dictionary of grain interface types to their
        /// corresponding index information. The index information is
        /// a dictionary from index IDs defined on a grain interface to
        /// a triple. The triple consists of: 1) the index object (that
        /// implements IndexInterface, 2) the IndexMetaData object for
        /// this index, and 3) the IndexUpdateGenerator instance for this index.
        /// This triple is untyped, because IndexInterface, IndexMetaData
        /// and IndexUpdateGenerator types are not visible in this project.
        /// 
        /// This method returns an empty dictionary if the OrleansIndexing 
        /// project is not available.
        /// </returns>
        public IDictionary<Type, IDictionary<string, Tuple<object, object, object>>> GetGrainClassIndexes(bool strict)
        {
            var result = new Dictionary<Type, IDictionary<string, Tuple<object, object, object>>>();
            try
            {
                //iIndexableGrainType = Type.GetType("Orleans.Indexing.IIndexableGrain, OrleansIndexing");
                genericIIndexableGrainType = Type.GetType("Orleans.Indexing.IIndexableGrain`1" + AssemblySeparator + OrleansIndexingAssembly);
                genericFaultTolerantIndexableGrainType = Type.GetType("Orleans.Indexing.IndexableGrain`1" + AssemblySeparator + OrleansIndexingAssembly);
                indexAttributeType = Type.GetType("Orleans.Indexing.IndexAttribute" + AssemblySeparator + OrleansIndexingAssembly);
                indexTypeProperty = indexAttributeType.GetProperty("IndexType");
                indexFactoryType = Type.GetType("Orleans.Indexing.IndexFactory" + AssemblySeparator + OrleansIndexingAssembly);
                createIndexMethod = (Func<IGrainFactory, Type, string, bool, bool, int, PropertyInfo, Tuple<object, object, object>>)Delegate.CreateDelegate(
                                        typeof(Func<IGrainFactory, Type, string, bool, bool, int, PropertyInfo, Tuple<object, object, object>>),
                                        indexFactoryType.GetMethod("CreateIndex", BindingFlags.Static | BindingFlags.NonPublic));
                registerIndexWorkflowQueuesMethod = (Action<Type, Type>)Delegate.CreateDelegate(
                                        typeof(Action<Type, Type>),
                                        indexFactoryType.GetMethod("RegisterIndexWorkflowQueues", BindingFlags.Static | BindingFlags.NonPublic));
                isEagerProperty = indexAttributeType.GetProperty("IsEager");
                isUniqueProperty = indexAttributeType.GetProperty("IsUnique");
                maxEntriesPerBucketProperty = indexAttributeType.GetProperty("MaxEntriesPerBucket");
                totalIndexType = Type.GetType("Orleans.Indexing.TotalIndex" + AssemblySeparator + OrleansIndexingAssembly);
            }
            catch
            {
                //indexing project is not added as a dependency.
                return result;
            }

            Type[] grainTypes = strict
                ? TypeUtils.GetTypes(TypeUtils.IsConcreteGrainClass, logger).ToArray()
                : TypeUtils.GetTypes(discoveredAssemblyLocations, TypeUtils.IsConcreteGrainClass, logger).ToArray();

            //for all discovered grain types
            foreach (var grainType in grainTypes)
            {
                if (result.ContainsKey(grainType))
                    throw new InvalidOperationException(
                        string.Format("Precondition violated: GetLoadedGrainTypes should not return a duplicate type ({0})", TypeUtils.GetFullName(grainType)));
                GetIndexesForASingleGrainType(result, grainType);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetIndexesForASingleGrainType(Dictionary<Type, IDictionary<string, Tuple<object, object, object>>> result, Type grainType)
        {
            Type[] interfaces = grainType.GetInterfaces();
            int numInterfaces = interfaces.Length;

            //iterate over the interfaces of the grain type
            for (int i = 0; i < numInterfaces; ++i)
            {
                Type iIndexableGrain = interfaces[i];

                //if the interface extends IIndexable<TProperties> interface
                if (iIndexableGrain.IsGenericType && iIndexableGrain.GetGenericTypeDefinition() == genericIIndexableGrainType)
                {
                    Type propertiesArg = iIndexableGrain.GetGenericArguments()[0];
                    //and if TProperties is a class
                    if (propertiesArg.GetTypeInfo().IsClass)
                    {
                        //then, the indexes are added to all the descendant
                        //interfaces of IIndexable<TProperties>, which are
                        //defined by end-users
                        for (int j = 0; j < numInterfaces; ++j)
                        {
                            Type userDefinedIGrain = interfaces[j];
                            CreateIndexesForASingleInterfaceOfAGrainType(result, iIndexableGrain, propertiesArg, userDefinedIGrain, grainType);
                        }
                    }
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CreateIndexesForASingleInterfaceOfAGrainType(Dictionary<Type, IDictionary<string, Tuple<object, object, object>>> result, Type iIndexableGrain, Type propertiesArg, Type userDefinedIGrain, Type userDefinedGrainImpl)
        {
            //checks whether the given interface is a user-defined
            //interface extending IIndexable<TProperties>
            if (iIndexableGrain != userDefinedIGrain && iIndexableGrain.IsAssignableFrom(userDefinedIGrain) && !result.ContainsKey(userDefinedIGrain))
            {
                //check either all indexes are defined as lazy
                //or all indexes are defined as lazy and none of them
                //are Total Index, because Total Indexes cannot be lazy
                CheckAllIndexesAreEitherLazyOrEager(propertiesArg, userDefinedIGrain, userDefinedGrainImpl);

                IDictionary<string, Tuple<object, object, object>> indexesOnGrain = new Dictionary<string, Tuple<object, object, object>>();
                //all the properties in TProperties are scanned for Index
                //annotation and the index is created using the information
                //provided in the annotation
                bool isEagerlyUpdated = true;
                foreach (PropertyInfo p in propertiesArg.GetProperties())
                {
                    var indexAttrs = p.GetCustomAttributes(indexAttributeType, false);
                    foreach (var indexAttr in indexAttrs)
                    {
                        string indexName = "__" + p.Name;
                        Type indexType = (Type)indexTypeProperty.GetValue(indexAttr);
                        if (indexType.IsGenericTypeDefinition)
                        {
                            indexType = indexType.MakeGenericType(p.PropertyType, userDefinedIGrain);
                        }

                        //if it's not eager, then it's configured to be lazily updated
                        isEagerlyUpdated = (bool)isEagerProperty.GetValue(indexAttr);
                        bool isUnique = (bool)isUniqueProperty.GetValue(indexAttr);
                        int maxEntriesPerBucket = (int)maxEntriesPerBucketProperty.GetValue(indexAttr);
                        indexesOnGrain.Add(indexName, createIndexMethod(InsideRuntimeClient.Current.ConcreteGrainFactory, indexType, indexName, isUnique, isEagerlyUpdated, maxEntriesPerBucket, p));
                    }
                }
                result.Add(userDefinedIGrain, indexesOnGrain);
                if (!isEagerlyUpdated)
                {
                    registerIndexWorkflowQueuesMethod(userDefinedIGrain, userDefinedGrainImpl);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckAllIndexesAreEitherLazyOrEager(Type propertiesArg, Type userDefinedIGrain, Type userDefinedGrainImpl)
        {
            bool isFaultTolerant = TypeUtils.IsSubclassOfRawGenericType(genericFaultTolerantIndexableGrainType, userDefinedGrainImpl);
            foreach (PropertyInfo p in propertiesArg.GetProperties())
            {
                var indexAttrs = p.GetCustomAttributes(indexAttributeType, false);
                bool isFirstIndexEager = false;
                if (indexAttrs.Count() > 0)
                {
                    isFirstIndexEager = (bool)isEagerProperty.GetValue(indexAttrs[0]);
                }
                foreach (var indexAttr in indexAttrs)
                {
                    bool isEager = (bool)isEagerProperty.GetValue(indexAttr);
                    Type indexType = (Type)indexTypeProperty.GetValue(indexAttr);
                    bool isTotalIndex = totalIndexType.IsAssignableFrom(indexType);

                    //Total Index cannot be configured as being lazy
                    if (isTotalIndex && isEager)
                    {
                        throw new InvalidOperationException(string.Format("A Total Index cannot be configured to be updated eagerly. The only option for updating a Total Index is lazy updating. Total Index of type {0} is defined to be updated eagerly on property {1} of class {2} on {3} grain interface.", TypeUtils.GetFullName(indexType), p.Name, TypeUtils.GetFullName(propertiesArg), TypeUtils.GetFullName(userDefinedIGrain)));
                    }
                    else if(isFaultTolerant && isEager)
                    {
                        throw new InvalidOperationException(string.Format("A fault-tolerant grain implementation cannot be configured to eagerly update its indexes. The only option for updating the indexes of a fault-tolerant indexable grain is lazy updating. The index of type {0} is defined to be updated eagerly on property {1} of class {2} on {3} grain implementation class.", TypeUtils.GetFullName(indexType), p.Name, TypeUtils.GetFullName(propertiesArg), TypeUtils.GetFullName(userDefinedGrainImpl)));
                    }
                    else if (isEager != isFirstIndexEager)
                    {
                        throw new InvalidOperationException(string.Format("Some indexes on property class {0} of {1} grain interface are defined to be updated eagerly while others are configured as lazy updating. You should fix this by configuring all indexes to be updated lazily or eagerly. If you have at least one Total Index among your indexes, then all other indexes should be configured as lazy, too.", TypeUtils.GetFullName(propertiesArg), TypeUtils.GetFullName(userDefinedIGrain)));
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<int, Type>> GetGrainMethodInvokerTypes(bool strict)
        {
            var result = new Dictionary<int, Type>();
            Type[] types = strict
                ? TypeUtils.GetTypes(TypeUtils.IsGrainMethodInvokerType, logger).ToArray()
                : TypeUtils.GetTypes(discoveredAssemblyLocations, TypeUtils.IsGrainMethodInvokerType, logger).ToArray();

            foreach (var type in types)
            {
                var attrib = type.GetTypeInfo().GetCustomAttribute<MethodInvokerAttribute>(true);
                int ifaceId = attrib.InterfaceId;

                if (result.ContainsKey(ifaceId))
                    throw new InvalidOperationException(string.Format("Grain method invoker classes {0} and {1} use the same interface id {2}", result[ifaceId].FullName, type.FullName, ifaceId));

                result[ifaceId] = type;
            }
            return result;
        }

        /// <summary>
        /// Get type data for the given grain type
        /// </summary>
        private static GrainTypeData GetTypeData(Type grainType, Type stateObjectType)
        {
            return grainType.GetTypeInfo().IsGenericTypeDefinition ?
                new GenericGrainTypeData(grainType, stateObjectType) :
                new GrainTypeData(grainType, stateObjectType);
        }

        private static void LogGrainTypesFound(LoggerImpl logger, Dictionary<string, GrainTypeData> grainTypeData)
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("Loaded grain type summary for {0} types: ", grainTypeData.Count));

            foreach (var grainType in grainTypeData.Values.OrderBy(gtd => gtd.Type.FullName))
            {
                // Skip system targets and Orleans grains
                var assemblyName = grainType.Type.GetTypeInfo().Assembly.FullName.Split(',')[0];
                if (!typeof(ISystemTarget).IsAssignableFrom(grainType.Type))
                {
                    int grainClassTypeCode = CodeGeneration.GrainInterfaceUtils.GetGrainClassTypeCode(grainType.Type);
                    sb.AppendFormat("Grain class {0}.{1} [{2} (0x{3})] from {4}.dll implementing interfaces: ",
                        grainType.Type.Namespace,
                        TypeUtils.GetTemplatedName(grainType.Type),
                        grainClassTypeCode,
                        grainClassTypeCode.ToString("X"),
                        assemblyName);
                    bool first = true;

                    foreach (var iface in grainType.RemoteInterfaceTypes)
                    {
                        if (!first)
                            sb.Append(", ");

                        sb.Append(iface.Namespace).Append(".").Append(TypeUtils.GetTemplatedName(iface));

                        if (CodeGeneration.GrainInterfaceUtils.IsGrainType(iface))
                        {
                            int ifaceTypeCode = CodeGeneration.GrainInterfaceUtils.GetGrainInterfaceId(iface);
                            sb.AppendFormat(" [{0} (0x{1})]", ifaceTypeCode, ifaceTypeCode.ToString("X"));
                        }
                        first = false;
                    }
                    sb.AppendLine();
                }
            }
            var report = sb.ToString();
            logger.LogWithoutBulkingAndTruncating(Severity.Info, ErrorCode.Loader_GrainTypeFullList, report);
        }
    }
}
