using System.Threading.Tasks;
using System.Collections.Generic;
using UnitTests.GrainInterfaces;
using Xunit;
using Orleans;
using Orleans.Indexing;
using Xunit.Abstractions;
using System.Threading;
using TestExtensions;

namespace UnitTests.IndexingTests
{
    public class FaultTolerantIndexingTestsTwoSilos : HostedTestClusterEnsureDefaultStarted
    {

        private readonly ITestOutputHelper output;
        private const int DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY = 1000; //one second delay for writes to the in-memory indexes should be enough

        public FaultTolerantIndexingTestsTwoSilos(DefaultClusterFixture fixture, ITestOutputHelper output)
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

            IPlayer1Grain p1 = GrainClient.GrainFactory.GetGrain<IPlayer1Grain>(1);
            await p1.SetLocation("Seattle");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__GetLocation");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer1Grain p2 = GrainClient.GrainFactory.GetGrain<IPlayer1Grain>(2);
            IPlayer1Grain p3 = GrainClient.GrainFactory.GetGrain<IPlayer1Grain>(3);

            await p2.SetLocation("Seattle");
            await p3.SetLocation("San Fransisco");

            IndexInterface<string, IPlayer1Grain> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer1Grain>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer1Grain, Player1Properties>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            await p2.Deactivate();

            await Task.Delay(DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer1Grain, Player1Properties>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer1Grain>(2);
            Assert.Equal("Seattle", await p2.GetLocation());

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer1Grain, Player1Properties>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));
        }

        /// <summary>
        /// Tests basic functionality of AHashIndexPartitionedPerSiloImpl with 1 Silo
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup2()
        {
            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            IPlayer2Grain p1 = GrainClient.GrainFactory.GetGrain<IPlayer2Grain>(1);
            await p1.SetLocation("Tehran");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__GetLocation");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer2Grain p2 = GrainClient.GrainFactory.GetGrain<IPlayer2Grain>(2);
            IPlayer2Grain p3 = GrainClient.GrainFactory.GetGrain<IPlayer2Grain>(3);

            await p2.SetLocation("Tehran");
            await p3.SetLocation("Yazd");

            IndexInterface<string, IPlayer2Grain> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer2Grain>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            output.WriteLine("Before check 1");
            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2Grain, Player2Properties>("Tehran", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            await p2.Deactivate();

            await Task.Delay(DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY);

            output.WriteLine("Before check 2");
            Assert.Equal(1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2Grain, Player2Properties>("Tehran", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer2Grain>(2);
            output.WriteLine("Before check 3");
            Assert.Equal("Tehran", await p2.GetLocation());

            output.WriteLine("Before check 4");
            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2Grain, Player2Properties>("Tehran", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));
            output.WriteLine("Done.");
        }

        /// <summary>
        /// Tests basic functionality of HashIndexPartitionedPerKey
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup4()
        {
            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            IPlayer3Grain p1 = GrainClient.GrainFactory.GetGrain<IPlayer3Grain>(1);
            await p1.SetLocation("Seattle");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__Location");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer3Grain p2 = GrainClient.GrainFactory.GetGrain<IPlayer3Grain>(2);
            IPlayer3Grain p3 = GrainClient.GrainFactory.GetGrain<IPlayer3Grain>(3);

            await p2.SetLocation("Seattle");
            await p3.SetLocation("San Fransisco");

            IndexInterface<string, IPlayer3Grain> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer3Grain>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer3Grain, Player3Properties>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            await p2.Deactivate();

            await Task.Delay(DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY);

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer3Grain, Player3Properties>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer3Grain>(2);
            Assert.Equal("Seattle", await p2.GetLocation());

            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer3Grain, Player3Properties>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));
        }

        /// <summary>
        /// Tests basic functionality of HashIndexPartitionedPerKey
        /// </summary>
        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task Test_Indexing_IndexLookup5()
        {

            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            //if (HostedCluster.SecondarySilos.Count == 0)
            //{
            //    HostedCluster.StartAdditionalSilos(1);
            //    await HostedCluster.WaitForLivenessToStabilizeAsync();
            //}

            //await GrainClient.GrainFactory.DropAllIndexes<IPlayerGrain>();

            IPlayer3Grain p1 = GrainClient.GrainFactory.GetGrain<IPlayer3Grain>(1);
            await p1.SetLocation("Seattle");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__Location");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer3Grain p2 = GrainClient.GrainFactory.GetGrain<IPlayer3Grain>(2);
            IPlayer3Grain p3 = GrainClient.GrainFactory.GetGrain<IPlayer3Grain>(3);
            IPlayer3Grain p4 = GrainClient.GrainFactory.GetGrain<IPlayer3Grain>(4);
            IPlayer3Grain p5 = GrainClient.GrainFactory.GetGrain<IPlayer3Grain>(5);

            await p2.SetLocation("Seattle");
            await p3.SetLocation("San Fransisco");
            await p4.SetLocation("Tehran");
            await p5.SetLocation("Yazd");

            for(int i = 0; i < 100; ++i)
            {
                List<Task> tasks = new List<Task>();
                for (int j = 0; j < 10; ++j)
                {
                    p1 = GrainClient.GrainFactory.GetGrain<IPlayer3Grain>(j);
                    tasks.Add(p1.SetLocation("Yazd" + i + "-" + j ));
                }
                await Task.WhenAll(tasks);
            }
        }
    }
}
