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
    public class FaultTolerantIndexingTestsSingleSilo : HostedTestClusterEnsureDefaultStarted
    {

        private readonly ITestOutputHelper output;
        private const int DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY = 1000; //one second delay for writes to the in-memory indexes should be enough

        public FaultTolerantIndexingTestsSingleSilo(DefaultClusterFixture fixture, ITestOutputHelper output)
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

            IPlayer2Grain p1 = GrainClient.GrainFactory.GetGrain<IPlayer2Grain>(1);
            await p1.SetLocation("Seattle");

            //bool isLocIndexCreated = await GrainClient.GrainFactory.CreateAndRegisterIndex<HashIndexSingleBucketInterface<string, IPlayerGrain>, PlayerLocIndexGen>("__GetLocation");
            //Assert.IsTrue(isLocIndexCreated);

            IPlayer2Grain p2 = GrainClient.GrainFactory.GetGrain<IPlayer2Grain>(2);
            IPlayer2Grain p3 = GrainClient.GrainFactory.GetGrain<IPlayer2Grain>(3);

            await p2.SetLocation("Seattle");
            await p3.SetLocation("San Fransisco");

            IndexInterface<string, IPlayer2Grain> locIdx = GrainClient.GrainFactory.GetIndex<string, IPlayer2Grain>("__Location");

            while (!await locIdx.IsAvailable()) Thread.Sleep(50);

            output.WriteLine("Before check 1");
            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2Grain, Player2Properties>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            await p2.Deactivate();

            await Task.Delay(DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY);

            output.WriteLine("Before check 2");
            Assert.Equal(1, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2Grain, Player2Properties>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));

            p2 = GrainClient.GrainFactory.GetGrain<IPlayer2Grain>(2);
            output.WriteLine("Before check 3");
            Assert.Equal("Seattle", await p2.GetLocation());

            output.WriteLine("Before check 4");
            Assert.Equal(2, await IndexingTestUtils.CountPlayersStreamingIn<IPlayer2Grain, Player2Properties>("Seattle", output, DELAY_UNTIL_INDEXES_ARE_UPDATED_LAZILY));
            output.WriteLine("Done.");
        }
    }
}
