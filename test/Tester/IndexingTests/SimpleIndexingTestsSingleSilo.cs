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

    public class SimpleIndexingTestsSingleSilo : HostedTestClusterEnsureDefaultStarted
    {

        private readonly ITestOutputHelper output;

        public SimpleIndexingTestsSingleSilo(DefaultClusterFixture fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        //[Fact, TestCategory("BVT"), TestCategory("Indexing")]
        //public async Task Test_Indexing_AddOneIndex()
        //{
        //    bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("locIdx1");
        //    Assert.IsTrue(isLocIndexCreated);
        //}

        //[Fact, TestCategory("BVT"), TestCategory("Indexing")]
        //public async Task Test_Indexing_AddTwoIndexes()
        //{
        //    bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("locIdx2");
        //    bool isScoreIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<int, IPlayerGrain>, PlayerScoreIndexGen>("scoreIdx2");

        //    Assert.IsTrue(isLocIndexCreated);
        //    Assert.IsTrue(isScoreIndexCreated);
        //}

        /// <summary>
        /// Tests basic functionality of HashIndexSingleBucket
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup1()
        {
            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            IPlayer1GrainNonFaultTolerant p1 = GrainClient.GrainFactory.GetGrain<IPlayer1GrainNonFaultTolerant>(1);
            await p1.SetLocation("Seattle");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__GetLocation");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer1GrainNonFaultTolerant p2 = GrainClient.GrainFactory.GetGrain<IPlayer1GrainNonFaultTolerant>(2);
            IPlayer1GrainNonFaultTolerant p3 = GrainClient.GrainFactory.GetGrain<IPlayer1GrainNonFaultTolerant>(3);

            await p2.SetLocation("Seattle");
            await p3.SetLocation("San Fransisco");

            IndexInterface<string, IPlayer1GrainNonFaultTolerant> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer1GrainNonFaultTolerant>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer1GrainNonFaultTolerant, Player1PropertiesNonFaultTolerant>("Seattle", output));

            await p2.Deactivate();

            Thread.Sleep(1000);

            Assert.Equal(1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer1GrainNonFaultTolerant, Player1PropertiesNonFaultTolerant>("Seattle", output));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer1GrainNonFaultTolerant>(2);
            Assert.Equal("Seattle", await p2.GetLocation());

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer1GrainNonFaultTolerant, Player1PropertiesNonFaultTolerant>("Seattle", output));
        }

        /// <summary>
        /// Tests basic functionality of AHashIndexPartitionedPerSiloImpl with 1 Silo
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
