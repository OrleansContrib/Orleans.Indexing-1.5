using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnitTests.GrainInterfaces;
using Xunit;
using Orleans;
using Orleans.Indexing;
using Xunit.Abstractions;
using TestExtensions;

namespace UnitTests.IndexingTests
{


    public class ActiveGrainEnumerationTests : HostedTestClusterEnsureDefaultStarted
    {

        private readonly ITestOutputHelper output;

        public ActiveGrainEnumerationTests(DefaultClusterFixture fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task FindActiveGrains()
        {

            //IManagementGrain mgmtGrain = GrainClient.GrainFactory.GetGrain<IManagementGrain>(RuntimeInterfaceConstants.SYSTEM_MANAGEMENT_ID);
            //Catalog.GetGrainStatistics();
            // start a second silo and wait for it
            if (HostedCluster.SecondarySilos.Count == 0)
            {
                HostedCluster.StartAdditionalSilo();
                await HostedCluster.WaitForLivenessToStabilizeAsync();
            }

            // create grains
            output.WriteLine("creating and activating grains");
            var grain1 = GrainClient.GrainFactory.GetGrain<ISimpleGrain>(1);
            var grain2 = GrainClient.GrainFactory.GetGrain<ISimpleGrain>(2);
            var grain3 = GrainClient.GrainFactory.GetGrain<ISimpleGrain>(3);
            await grain1.GetA();
            await grain2.GetA();
            await grain3.GetA();

            //enumerate active grains
            output.WriteLine("\n\nour own grain statistics");
            IActiveGrainEnumeratorGrain enumGrain = GrainClient.GrainFactory.GetGrain<IActiveGrainEnumeratorGrain>(0);
            IEnumerable<Guid> activeGrains = enumGrain.GetActiveGrains("UnitTests.Grains.SimpleGrain").Result;
            foreach (var entry in activeGrains)
            {
                output.WriteLine("guid = {0}", entry);
            }

            Assert.Equal(3, activeGrains.AsQueryable().Count());
        }

        [Fact, TestCategory("BVT"), TestCategory("Indexing")]
        public async Task FindActiveGrains_typed()
        {

            //IManagementGrain mgmtGrain = GrainClient.GrainFactory.GetGrain<IManagementGrain>(RuntimeInterfaceConstants.SYSTEM_MANAGEMENT_ID);
            //Catalog.GetGrainStatistics();
            // start a second silo and wait for it
            if (HostedCluster.SecondarySilos.Count == 0)
            {
                HostedCluster.StartAdditionalSilo();
                await HostedCluster.WaitForLivenessToStabilizeAsync();
            }

            // create grains
            output.WriteLine("creating and activating grains");
            var grain1 = GrainClient.GrainFactory.GetGrain<ISimpleGrain>(1);
            var grain2 = GrainClient.GrainFactory.GetGrain<ISimpleGrain>(2);
            var grain3 = GrainClient.GrainFactory.GetGrain<ISimpleGrain>(3);
            await grain1.GetA();
            await grain2.GetA();
            await grain3.GetA();

            //enumerate active grains
            output.WriteLine("\n\nour own grain statistics");
            IActiveGrainEnumeratorGrain enumGrain = GrainClient.GrainFactory.GetGrain<IActiveGrainEnumeratorGrain>(0);
            IEnumerable<IGrain> activeGrains = enumGrain.GetActiveGrains(typeof(ISimpleGrain)).Result;
            foreach (var entry in activeGrains)
            {
                output.WriteLine("guid = {0}", entry.GetPrimaryKey());
            }

            Assert.Equal(3, activeGrains.AsQueryable().Count());
        }
    }
}
