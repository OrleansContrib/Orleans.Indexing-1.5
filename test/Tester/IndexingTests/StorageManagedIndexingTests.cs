using System.Threading.Tasks;
using System.Collections.Generic;
using UnitTests.GrainInterfaces;
using Xunit;
using Orleans;
using Xunit.Abstractions;
using Orleans.TestingHost;
using Orleans.Runtime.Configuration;
using TestExtensions;

namespace UnitTests.IndexingTests
{
    public class StorageManagedIndexingTests : OrleansTestingBase, IClassFixture<StorageManagedIndexingTests.Fixture>
    {
        public class Fixture : BaseTestClusterFixture
        {
            protected override TestCluster CreateTestCluster()
            {
                var options = new TestClusterOptions();
                options.ClusterConfiguration.AddMemoryStorageProvider("Default");
                options.ClusterConfiguration.AddMemoryStorageProvider("MemoryStore");

                //Required for IndexingTests
                options.ClusterConfiguration.AddMemoryStorageProvider("PubSubStore");
                options.ClusterConfiguration.AddMemoryStorageProvider("IndexingStorageProvider");
                options.ClusterConfiguration.AddMemoryStorageProvider("IndexingWorkflowQueueStorageProvider");
                options.ClusterConfiguration.AddSimpleMessageStreamProvider("IndexingStreamProvider");
                options.ClientConfiguration.AddSimpleMessageStreamProvider("IndexingStreamProvider");

                string DocumentDBURL = "https://YOUR_DOCUMENTDB_URL.documents.azure.com:443/";
                string DocumentDBKey = "YOUR_DOCUMENTDB_KEY";
                string DocumentDBDatabase = "testtest";
                string DocumentDBOfferType = "10100"; //if V1 => S1, S2 or S3 else if V2 => RU as integer
                string DocumentDBIndexingMode = "consistent";
                //options.ClusterConfiguration.AddDocumentDBStorageProvider("DocumentDBStore", DocumentDBURL, DocumentDBKey, DocumentDBDatabase, DocumentDBOfferType, DocumentDBIndexingMode);
                options.ClusterConfiguration.AddMemoryStorageProvider("DocumentDBStore");
                //options.ClusterConfiguration.Defaults.DefaultTraceLevel = Severity.Verbose;
                //options.ClientConfiguration.DefaultTraceLevel = Severity.Verbose;
                //options.ClusterConfiguration.GetOrCreateNodeConfigurationForSilo("Primary").DefaultTraceLevel = Severity.Verbose;
                return new TestCluster(options);
            }
        }

        private readonly ITestOutputHelper output;
        private const int DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY = 1000; //one second delay for writes to the in-memory indexes should be enough
        
        public StorageManagedIndexingTests(Fixture fixture, ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Tests basic functionality of HashIndexSingleBucker
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_StorageManagedIndexing_IndexLookup1()
        {
            IPlayerDocDBGrain p1 = GrainClient.GrainFactory.GetGrain<IPlayerDocDBGrain>(1);
            await p1.SetLocation("Yazd");
            IPlayerDocDBGrain p2 = GrainClient.GrainFactory.GetGrain<IPlayerDocDBGrain>(2);
            await p2.SetLocation("Tehran");
            List<Task> tasks = new List<Task>();
            IPlayerDocDBGrain p3;
            int numPlaces = 5;
            for(int j = 1; j < numPlaces; ++j) {
                p3 = GrainClient.GrainFactory.GetGrain<IPlayerDocDBGrain>(j + 3);
                tasks.Add( p3.SetLocation("Tehran") );
                tasks.Add( p3.SetLocation("Kashan") );
                for (int i = 0; i < 5; ++i)
                {
                    tasks.Add( p3.SetLocation("Tehran " + i) );
                }

                tasks.Add( p3.SetLocation("Yazd") );
            }
            await Task.WhenAll(tasks);
            //Assert.Equal(numPlaces + 1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayerDocDBGrain, PlayerDocDBProperties>("Yazd", output));
        }
    }
}
