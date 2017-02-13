using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.Providers.Azure;
using Orleans.Serialization;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;

namespace Orleans.StorageProvider.DocumentDB
{
    //authors: Jakub Konecki (jkonecki@gmail.com) and Mohammad Dashti (mdashti@gmail.com)
    public class DocumentDBStorageProvider : IStorageProvider
    {
        internal const string URL = "Url";
        internal const string KEY = "Key";
        internal const string INDEXING_MODE = "IndexingMode";
        internal const string INDEXING_MODE_CONSISTENT = "consistent";
        internal const string INDEXING_MODE_LAZY = "lazy";
        internal const string DATABASE = "Database";
        //internal const string OFFER_TYPE_VERSION = "OfferTypeVersion";
        internal const string OFFER_TYPE = "OfferType";

        public string Name { get; set; }
        public Logger Log { get; set; }

        private DocumentClient Client { get; set; }
        private Database Database { get; set; }

        private string DatabaseName { get; set; }

        private string IndexMode { get; set; }

        private string OfferType { get; set; }

        //private string OfferTypeVersion { get; set; }

        private JsonSerializerSettings jsonSettings;

        private ConcurrentDictionary<string, bool> existingCollections;
        private ConcurrentDictionary<string, bool> createdCollections;

        private bool isFaultTolerant;

        public async Task Init(string name, Providers.IProviderRuntime providerRuntime, Providers.IProviderConfiguration config)
        {
            existingCollections = new ConcurrentDictionary<string, bool>();
            createdCollections = new ConcurrentDictionary<string, bool>();
            Log = providerRuntime.GetLogger("Storage.DocumentDBStorageProvider");
            try
            {
                isFaultTolerant = false; //by default, unless we found otherwise later

                this.jsonSettings = OrleansJsonSerializer.UpdateSerializerSettings(OrleansJsonSerializer.GetDefaultSerializerSettings(), config);
                var url = config.Properties[URL];
                var key = config.Properties[KEY];
                //OfferTypeVersion = config.Properties[OFFER_TYPE_VERSION]; //V1 or V2
                OfferType = config.Properties[OFFER_TYPE]; //if V1 => S1, S2, S3 else if V2 => RU as integer
                DatabaseName = config.Properties[DATABASE];
                IndexMode = config.Properties.ContainsKey(INDEXING_MODE) ? config.Properties[INDEXING_MODE] : INDEXING_MODE_CONSISTENT;
                this.Client = new DocumentClient(new Uri(url), key, new ConnectionPolicy
                {
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp
                });

                await this.Client.OpenAsync();

                var databases = await this.Client.ReadDatabaseFeedAsync();
                this.Database = databases.Where(d => d.Id == DatabaseName).FirstOrDefault()
                    ?? await this.Client.CreateDatabaseAsync(new Database { Id = DatabaseName });

            }
            catch (Exception ex)
            {
                Log.Error((int)AzureProviderErrorCode.DocumentDBProvider_InitProvider, "Error in DocumentDBStorageProvider Init.", ex);
            }
        }

        public Task Close()
        {
            this.Client.Dispose();

            return TaskDone.Done;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try
            {
                await this.EnsureCollection(grainType);
                var documentId = grainReference.ToKeyString();
                Document document = await this.Client.ReadDocumentAsync(
                                        UriFactory.CreateDocumentUri(this.DatabaseName, grainType, documentId),
                                        new RequestOptions { PartitionKey = new PartitionKey(documentId) }
                                    );
                if (document != null)
                {
                    GrainStateDocument gDoc = (dynamic) document;
                    grainState.State = ((JObject)gDoc.State).ToObject(grainState.State.GetType());
                    grainState.ETag = document.ETag;
                }
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                {
                    //it's normal that no state exists for a grain when it is initialized
                }
                else
                {
                    Log.Error((int)AzureProviderErrorCode.DocumentDBProvider_ReadError,
                        string.Format("Error reading: GrainType={0} Grainid={1} with Exception={2}", grainType, grainReference, dce.Message),
                        dce);

                    throw;
                }
            }
            catch (Exception ex)
            {
                Log.Error((int)AzureProviderErrorCode.DocumentDBProvider_ReadError,
                    string.Format("Error reading: GrainType={0} Grainid={1} with Exception={2}", grainType, grainReference, ex.Message),
                    ex);

                throw;
            }
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try
            {
                await this.EnsureCollection(grainType);
                var documentId = grainReference.ToKeyString();
                
                var ac = new AccessCondition { Condition = grainState.ETag, Type = AccessConditionType.IfMatch };
                Document document = await this.Client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(this.DatabaseName, grainType), new GrainStateDocument { Id = documentId, State = grainState.State, ETag = grainState.ETag }, new RequestOptions { AccessCondition = ac });

                if (document != null)
                {
                    GrainStateDocument gDoc = (dynamic)document;
                    grainState.ETag = document.ETag;
                }
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.PreconditionFailed) 
                { 
                    Log.Error((int)AzureProviderErrorCode.DocumentDBProvider_WriteEtagError,
                        string.Format("Error writing because of a conflict on E-Tag: GrainType={0} Grainid={1} ETag={2} with Exception={3}", grainType, grainReference, grainState.ETag, dce.Message),
                        dce);
                }
                throw new Exception("DocumentClientException occured with the following message: " + dce.Message);
            }
            catch (Exception ex)
            {
                Log.Error((int)AzureProviderErrorCode.DocumentDBProvider_WriteError,
                    string.Format("Error writing: GrainType={0} Grainid={1} ETag={2} with Exception={3}", grainType, grainReference, grainState.ETag, ex.Message),
                    ex);

                throw;
            }
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try
            {
                await this.EnsureCollection(grainType);
                var documentId = grainReference.ToKeyString();
                Document document = await this.Client.ReadDocumentAsync(
                                        UriFactory.CreateDocumentUri(this.DatabaseName, grainType, documentId),
                                        new RequestOptions { PartitionKey = new PartitionKey(documentId) }
                                    );
                if (document != null)
                {
                    await this.Client.DeleteDocumentAsync(document.SelfLink);
                }
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                {
                    //it's normal that no state exists for a grain when it is initialized
                }
                else
                {
                    Log.Error((int)AzureProviderErrorCode.DocumentDBProvider_ReadError,
                        string.Format("Error reading: GrainType={0} Grainid={1} with Exception={2}", grainType, grainReference, dce.Message),
                        dce);

                    throw;
                }
            }
            catch (Exception ex)
            {
                Log.Error((int)AzureProviderErrorCode.DocumentDBProvider_ReadError,
                    string.Format("Error reading: GrainType={0} Grainid={1} with Exception={2}", grainType, grainReference, ex.Message),
                    ex);

                throw;
            }
        }

        public async Task<List<GrainReference>> Lookup<K>(string grainType, string indexedField, K key)
        {
            await EnsureCollection(grainType);
            FeedOptions queryOptions = new FeedOptions { EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = 10, MaxItemCount = -1 };

            // Now execute the same query via direct SQL
            IQueryable<object> sqlQuery = this.Client.CreateDocumentQuery<object>(
                    UriFactory.CreateDocumentCollectionUri(this.DatabaseName, grainType),
                    "SELECT c.id FROM c WHERE c.State."+ (isFaultTolerant ? "UserState." : "") + indexedField + " = " + (IsNumericType(typeof(K)) ? key.ToString() : ("'" + key + "'")),
                    queryOptions);
            var queryRes = await Task.Run(() => sqlQuery.AsEnumerable().ToList());

            List<GrainReference> idList = new List<GrainReference>();
            foreach (var elem in queryRes)
            {
                dynamic elemDyn = elem;
                idList.Add(GrainReference.FromKeyString(elemDyn.id));
            }

            return idList;
        }

        private async Task EnsureCollection(string collectionId)
        {
            if (!existingCollections.TryAdd(collectionId, true))
            {
                bool res;
                while(!createdCollections.TryGetValue(collectionId, out res))
                {
                    await Task.Delay(100);
                }
            }
            else
            {
                //var collections = await this.Client.ReadDocumentCollectionFeedAsync(this.Database.CollectionsLink);
                //docCol = collections.Where(c => c.Id == collectionId).FirstOrDefault();

                DocumentCollection docCol = await ExecuteWithRetries(
                        this.Client,
                        () => Task.Run(() => this.Client.CreateDocumentCollectionQuery(this.Database.CollectionsLink)
                                                    .Where(c => c.Id == collectionId).ToArray().SingleOrDefault()));

                if (docCol == null)
                {
                    docCol = new DocumentCollection { Id = collectionId };
                    string partition;
                    if (!CreateOrUpdateIndexes(collectionId, docCol, out partition))
                    {
                        // partition = "/id";
                    }
                    partition = "/id";
                    docCol.PartitionKey.Paths.Add(partition);

                    docCol = await CreateDocumentCollectionWithRetriesAsync(this.Client, this.Database, docCol, int.Parse(OfferType));
                }

                createdCollections.TryAdd(collectionId, true);
            }
        }

        /// <summary>
        /// Create a DocumentCollection, and retries if throttled.
        /// </summary>
        /// <param name="client">The DocumentDB client instance.</param>
        /// <param name="database">The database to use.</param>
        /// <param name="collectionDefinition">The collection definition to use.</param>
        /// <param name="offerThroughput">The offer throughput for the collection.</param>
        /// <returns>The created DocumentCollection.</returns>
        public static async Task<DocumentCollection> CreateDocumentCollectionWithRetriesAsync(
            DocumentClient client,
            Database database,
            DocumentCollection collectionDefinition,
            int offerThroughput)
        {
            return await ExecuteWithRetries(
                client,
                () => client.CreateDocumentCollectionAsync(
                        database.SelfLink,
                        collectionDefinition,
                        new RequestOptions { OfferThroughput = offerThroughput }));
        }



        /// <summary>
        /// Execute the function with retries on throttle.
        /// </summary>
        /// <typeparam name="V">The type of return value from the execution.</typeparam>
        /// <param name="client">The DocumentDB client instance.</param>
        /// <param name="function">The function to execute.</param>
        /// <returns>The response from the execution.</returns>
        public static async Task<V> ExecuteWithRetries<V>(DocumentClient client, Func<Task<V>> function)
        {
            TimeSpan sleepTime = TimeSpan.Zero;

            while (true)
            {
                try
                {
                    return await function();
                }
                catch (DocumentClientException de)
                {
                    if ((int)de.StatusCode != 429 && (int)de.StatusCode != 449)
                    {
                        throw;
                    }

                    sleepTime = de.RetryAfter;
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is DocumentClientException))
                    {
                        throw;
                    }

                    DocumentClientException de = (DocumentClientException)ae.InnerException;
                    if ((int)de.StatusCode != 429)
                    {
                        throw;
                    }

                    sleepTime = de.RetryAfter;
                    if (sleepTime < TimeSpan.FromMilliseconds(10))
                    {
                        sleepTime = TimeSpan.FromMilliseconds(10);
                    }
                }

                await Task.Delay(sleepTime);
            }
        }


        public static string OrleansIndexingAssembly = "OrleansIndexing";
        public static string AssemblySeparator = ", ";
        private static bool isIndexingAvailable = false;
        //private static Type iIndexableGrainType;
        private static Type genericIIndexableGrainType;
        private static Type DSMIndexAttributeType;
        private static PropertyInfo indexTypeProperty;
        private static Type genericFaultTolerantIndexableGrainType;

        private bool CreateOrUpdateIndexes(string grainTypeName, DocumentCollection col, out string partition)
        {
            partition = null;
            if(!isIndexingAvailable)
            {
                try
                {
                    //iIndexableGrainType = Type.GetType("Orleans.Indexing.IIndexableGrain, OrleansIndexing")
                    genericIIndexableGrainType = Type.GetType("Orleans.Indexing.IIndexableGrain`1" + AssemblySeparator + OrleansIndexingAssembly);
                    DSMIndexAttributeType = Type.GetType("Orleans.Indexing.DSMIndexAttribute" + AssemblySeparator + OrleansIndexingAssembly);
                    indexTypeProperty = DSMIndexAttributeType.GetProperty("IndexType");
                    genericFaultTolerantIndexableGrainType = Type.GetType("Orleans.Indexing.IndexableGrain`1" + AssemblySeparator + OrleansIndexingAssembly);

                    isIndexingAvailable = true;
                }
                catch
                {
                    //indexing classes are not available, thus indexing module is not available.
                    return false;
                }
            }

            Type grainType;
            if (!TypeUtils.TryResolveType(grainTypeName, out grainType))
            {
                Log.Error((int)AzureProviderErrorCode.DocumentDBProvider_TypeNotFound, string.Format("Could not find grain type = {0}", grainTypeName));
                return false;
            }
            isFaultTolerant = TypeUtils.IsSubclassOfRawGenericType(genericFaultTolerantIndexableGrainType, grainType);

            col.IndexingPolicy.IndexingMode = INDEXING_MODE_CONSISTENT.Equals(IndexMode) ? IndexingMode.Consistent : IndexingMode.Lazy;

            bool res = CreateDocumentDBIndexes(col, grainType, isFaultTolerant, out partition);

            col.IndexingPolicy.ExcludedPaths.Add(
                new ExcludedPath
                {
                    Path = "/"
                }
            );

            return res;
        }

        private static bool CreateDocumentDBIndexes(DocumentCollection col, Type grainType, bool isFaultTolerant, out string partition)
        {
            partition = null;
            Type[] interfaces = grainType.GetInterfaces();
            int numInterfaces = interfaces.Length;
            bool hasIndex = false;
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
                            hasIndex = CreateIndexForOneIndexedProperty(col, iIndexableGrain, propertiesArg, userDefinedIGrain, isFaultTolerant, out partition) || hasIndex;
                        }
                    }
                    break;
                }
            }
            return hasIndex;
        }

        private static bool CreateIndexForOneIndexedProperty(DocumentCollection col, Type iIndexableGrain, Type propertiesArg, Type userDefinedIGrain, bool isFaultTolerant, out string partition)
        {
            partition = null;
            bool hasIndex = false;
            //checks whether the given interface is a user-defined
            //interface extending IIndexable<TProperties>
            if (iIndexableGrain != userDefinedIGrain && iIndexableGrain.IsAssignableFrom(userDefinedIGrain))
            {
                IDictionary<string, Tuple<object, object, object>> indexesOnGrain = new Dictionary<string, Tuple<object, object, object>>();
                //all the properties in TProperties are scanned for Index
                //annotation and the index is created using the information
                //provided in the annotation
                foreach (PropertyInfo p in propertiesArg.GetProperties())
                {
                    var indexAttrs = p.GetCustomAttributes(DSMIndexAttributeType, false);
                    foreach (var indexAttr in indexAttrs)
                    {
                        string indexName = p.Name;
                        Type indexType = (Type)indexTypeProperty.GetValue(indexAttr);
                        //if (indexType.IsGenericTypeDefinition)
                        //{
                        //    indexType = indexType.MakeGenericType(p.PropertyType, userDefinedIGrain);
                        //}
                        //TODO it should be generalized
                        hasIndex = true;
                        partition = "/State/" + (isFaultTolerant ? "UserState/" : "") + indexName;
                        col.IndexingPolicy.IncludedPaths.Add(
                            new IncludedPath
                            {
                                Path = partition + "/*",
                                //Path = "/State/UserState/" + indexName + "/*",
                                Indexes = new Collection<Index> {
                                                        new HashIndex(IsNumericType(indexType) ? DataType.Number : DataType.String) { Precision = -1 }
                                }
                            }
                        );
                    }
                }
            }
            return hasIndex;
        }

        private static bool IsNumericType(Type o)
        {
            switch (Type.GetTypeCode(o))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        private class GrainStateDocument
        {
            [JsonProperty("id")]
            public string Id;
            public object State;
            public string ETag;
        }
    }
}