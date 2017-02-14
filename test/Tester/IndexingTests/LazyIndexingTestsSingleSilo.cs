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
    public class LazyIndexingTestsSingleSilo : HostedTestClusterEnsureDefaultStarted
    {

        private readonly ITestOutputHelper output;
        private const int DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY = 1000; //one second delay for writes to the in-memory indexes should be enough

        public LazyIndexingTestsSingleSilo(DefaultClusterFixture fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        /// <summary>
        /// Tests basic functionality of HashIndexSingleBucker
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup1()
        {
            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            IPlayer1GrainNonFaultTolerantLazy p1 = GrainClient.GrainFactory.GetGrain<IPlayer1GrainNonFaultTolerantLazy>(1);
            await p1.SetLocation("Seattle");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__GetLocation");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer1GrainNonFaultTolerantLazy p2 = GrainClient.GrainFactory.GetGrain<IPlayer1GrainNonFaultTolerantLazy>(2);
            IPlayer1GrainNonFaultTolerantLazy p3 = GrainClient.GrainFactory.GetGrain<IPlayer1GrainNonFaultTolerantLazy>(3);

            await p2.SetLocation("Seattle");
            await p3.SetLocation("San Fransisco");

            IndexInterface<string, IPlayer1GrainNonFaultTolerantLazy> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer1GrainNonFaultTolerantLazy>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);



            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer1GrainNonFaultTolerantLazy, Player1PropertiesNonFaultTolerantLazy>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            await p2.Deactivate();

            await Task.Delay(DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY);

            Assert.Equal(1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer1GrainNonFaultTolerantLazy, Player1PropertiesNonFaultTolerantLazy>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer1GrainNonFaultTolerantLazy>(2);
            Assert.Equal("Seattle", await p2.GetLocation());

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer1GrainNonFaultTolerantLazy, Player1PropertiesNonFaultTolerantLazy>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));
        }

        /// <summary>
        /// Tests basic functionality of AHashIndexPartitionedPerSiloImpl with 1 Silo
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup2()
        {
            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            IPlayer2GrainNonFaultTolerantLazy p1 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerantLazy>(1);
            await p1.SetLocation("Tehran");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__GetLocation");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer2GrainNonFaultTolerantLazy p2 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerantLazy>(2);
            IPlayer2GrainNonFaultTolerantLazy p3 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerantLazy>(3);

            await p2.SetLocation("Tehran");
            await p3.SetLocation("Yazd");

            IndexInterface<string, IPlayer2GrainNonFaultTolerantLazy> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer2GrainNonFaultTolerantLazy>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2GrainNonFaultTolerantLazy, Player2PropertiesNonFaultTolerantLazy>("Tehran", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            await p2.Deactivate();

            await Task.Delay(DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY);

            Assert.Equal(1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2GrainNonFaultTolerantLazy, Player2PropertiesNonFaultTolerantLazy>("Tehran", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerantLazy>(2);
            Assert.Equal("Tehran", await p2.GetLocation());

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2GrainNonFaultTolerantLazy, Player2PropertiesNonFaultTolerantLazy>("Tehran", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));
        }

        /// <summary>
        /// Tests basic functionality of HashIndexPartitionedPerKey
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup4()
        {
            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            IPlayer3GrainNonFaultTolerantLazy p1 = GrainClient.GrainFactory.GetGrain<IPlayer3GrainNonFaultTolerantLazy>(1);
            await p1.SetLocation("Seattle");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__Location");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer3GrainNonFaultTolerantLazy p2 = GrainClient.GrainFactory.GetGrain<IPlayer3GrainNonFaultTolerantLazy>(2);
            IPlayer3GrainNonFaultTolerantLazy p3 = GrainClient.GrainFactory.GetGrain<IPlayer3GrainNonFaultTolerantLazy>(3);

            await p2.SetLocation("Seattle");
            await p3.SetLocation("San Fransisco");

            IndexInterface<string, IPlayer3GrainNonFaultTolerantLazy> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer3GrainNonFaultTolerantLazy>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer3GrainNonFaultTolerantLazy, Player3PropertiesNonFaultTolerantLazy>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            await p2.Deactivate();

            await Task.Delay(DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY);

            Assert.Equal(1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer3GrainNonFaultTolerantLazy, Player3PropertiesNonFaultTolerantLazy>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer3GrainNonFaultTolerantLazy>(2);
            Assert.Equal("Seattle", await p2.GetLocation());

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer3GrainNonFaultTolerantLazy, Player3PropertiesNonFaultTolerantLazy>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));
        }
    }
}
