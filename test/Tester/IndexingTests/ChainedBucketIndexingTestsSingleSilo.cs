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
    //[Serializable]
    //public class PlayerLocIndexGen : IndexUpdateGenerator<string, PlayerProperties>
    //{
    //    public override string ExtractIndexImage(PlayerProperties g)
    //    {
    //        return g.Location;
    //    }
    //}

    //[Serializable]
    //public class PlayerScoreIndexGen : IndexUpdateGenerator<int, PlayerProperties>
    //{
    //    public override int ExtractIndexImage(PlayerProperties g)
    //    {
    //        return g.Score;
    //    }
    //}

    public class ChainedBucketIndexingTestsSingleSilo : HostedTestClusterEnsureDefaultStarted
    {

        private readonly ITestOutputHelper output;

        public ChainedBucketIndexingTestsSingleSilo(DefaultClusterFixture fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }
        
        /// <summary>
        /// Tests basic functionality of HashIndexSingleBucket with chained buckets
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup1()
        {
            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            IPlayerChain1Grain p1 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(1);
            await p1.SetLocation("Seattle");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__GetLocation");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayerChain1Grain p2 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(2);
            IPlayerChain1Grain p3 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(3);
            IPlayerChain1Grain p4 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(4);
            IPlayerChain1Grain p5 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(5);
            IPlayerChain1Grain p6 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(6);
            IPlayerChain1Grain p7 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(7);
            IPlayerChain1Grain p8 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(8);
            IPlayerChain1Grain p9 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(9);
            IPlayerChain1Grain p10 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(10);

            await p2.SetLocation("San Jose");
            await p3.SetLocation("San Fransisco");
            await p4.SetLocation("Bellevue");
            await p5.SetLocation("Redmond");
            await p6.SetLocation("Kirkland");
            await p7.SetLocation("Kirkland");
            await p8.SetLocation("Kirkland");
            await p9.SetLocation("Seattle");
            await p10.SetLocation("Kirkland");

            IndexInterface<string, IPlayerChain1Grain> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayerChain1Grain>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayerChain1Grain, PlayerChain1Properties>("Seattle", output));
            Assert.Equal(4, await IndexingTestUtils.CountPlayersStreamingIn<IPlayerChain1Grain, PlayerChain1Properties>("Kirkland", output));

            await p8.Deactivate();
            await p9.Deactivate();

            Thread.Sleep(1000);

            Assert.Equal(1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayerChain1Grain, PlayerChain1Properties>("Seattle", output));
            Assert.Equal(3, await IndexingTestUtils.CountPlayersStreamingIn<IPlayerChain1Grain, PlayerChain1Properties>("Kirkland", output));

            p10 = GrainClient.GrainFactory.GetGrain<IPlayerChain1Grain>(10);
            Assert.Equal("Kirkland", await p10.GetLocation());
        }

        /// <summary>
        /// Tests basic functionality of ActiveHashIndexPartitionedPerSiloImpl with 1 Silo
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup2()
        {
            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            IPlayer2GrainNonFaultTolerant p1 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerant>(1);
            await p1.SetLocation("Tehran");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__GetLocation");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer2GrainNonFaultTolerant p2 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerant>(2);
            IPlayer2GrainNonFaultTolerant p3 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerant>(3);

            await p2.SetLocation("Tehran");
            await p3.SetLocation("Yazd");

            IndexInterface<string, IPlayer2GrainNonFaultTolerant> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer2GrainNonFaultTolerant>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2GrainNonFaultTolerant, Player2PropertiesNonFaultTolerant>("Tehran", output));

            await p2.Deactivate();

            Thread.Sleep(1000);

            Assert.Equal(1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2GrainNonFaultTolerant, Player2PropertiesNonFaultTolerant>("Tehran", output));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer2GrainNonFaultTolerant>(2);
            Assert.Equal("Tehran", await p2.GetLocation());

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2GrainNonFaultTolerant, Player2PropertiesNonFaultTolerant>("Tehran", output));
        }

        /// <summary>
        /// Tests basic functionality of HashIndexPartitionedPerKey
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup4()
        {
            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            IPlayer3GrainNonFaultTolerant p1 = GrainClient.GrainFactory.GetGrain<IPlayer3GrainNonFaultTolerant>(1);
            await p1.SetLocation("Seattle");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__Location");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer3GrainNonFaultTolerant p2 = GrainClient.GrainFactory.GetGrain<IPlayer3GrainNonFaultTolerant>(2);
            IPlayer3GrainNonFaultTolerant p3 = GrainClient.GrainFactory.GetGrain<IPlayer3GrainNonFaultTolerant>(3);

            await p2.SetLocation("Seattle");
            await p3.SetLocation("San Fransisco");

            IndexInterface<string, IPlayer3GrainNonFaultTolerant> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer3GrainNonFaultTolerant>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer3GrainNonFaultTolerant, Player3PropertiesNonFaultTolerant>("Seattle", output));

            await p2.Deactivate();

            Thread.Sleep(1000);

            Assert.Equal(1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer3GrainNonFaultTolerant, Player3PropertiesNonFaultTolerant>("Seattle", output));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer3GrainNonFaultTolerant>(2);
            Assert.Equal("Seattle", await p2.GetLocation());

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer3GrainNonFaultTolerant, Player3PropertiesNonFaultTolerant>("Seattle", output));
        }
    }
}
