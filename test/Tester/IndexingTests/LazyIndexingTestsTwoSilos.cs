using System.Threading.Tasks;
using UnitTests.GrainInterfaces;
using Xunit;
using Orleans;
using Orleans.Indexing;
using Xunit.Abstractions;
using System.Threading;
using TestExtensions;

namespace UnitTests.IndexingTests
{
    public class LazyIndexingTestsTwoSilos : HostedTestClusterEnsureDefaultStarted
    {

        private readonly ITestOutputHelper output;
        private const int DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY = 1000; //one second delay for writes to the in-memory indexes should be enough

        public LazyIndexingTestsTwoSilos(DefaultClusterFixture fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        /// <summary>
        /// Tests basic functionality of ActiveHashIndexPartitionedPerSiloImpl with 2 Silos
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup3()
        {
            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            if (HostedCluster.SecondarySilos.Count == 0)
            {
                HostedCluster.StartAdditionalSilo();
                await HostedCluster.WaitForLivenessToStabilizeAsync();
            }

            IPlayer2GrainNonFaultTolerantLazy p1 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerantLazy>(1);
            await p1.SetLocation("Seattle");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__GetLocation");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer2GrainNonFaultTolerantLazy p2 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerantLazy>(2);
            IPlayer2GrainNonFaultTolerantLazy p3 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerantLazy>(3);

            await p2.SetLocation("Seattle");
            await p3.SetLocation("San Fransisco");

            IndexInterface<string, IPlayer2GrainNonFaultTolerantLazy> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer2GrainNonFaultTolerantLazy>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2GrainNonFaultTolerantLazy, Player2PropertiesNonFaultTolerantLazy>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            await p2.Deactivate();

            await Task.Delay(DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY);

            Assert.Equal(1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2GrainNonFaultTolerantLazy, Player2PropertiesNonFaultTolerantLazy>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerantLazy>(2);
            Assert.Equal("Seattle", await p2.GetLocation());

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2GrainNonFaultTolerantLazy, Player2PropertiesNonFaultTolerantLazy>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));
        }
    }
}
