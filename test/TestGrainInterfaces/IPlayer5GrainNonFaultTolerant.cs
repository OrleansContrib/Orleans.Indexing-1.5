using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player5PropertiesNonFaultTolerant : PlayerProperties
    {
        [Index(typeof(AHashIndexSingleBucket<string, IPlayer5GrainNonFaultTolerant>), IsEager: true, IsUnique: true)]
        public int Score { get; set; }

        [Index(typeof(AHashIndexPartitionedPerKey<string, IPlayer5GrainNonFaultTolerant>), IsEager: true, IsUnique: true)]
        public string Location { get; set; }
    }

    public interface IPlayer5GrainNonFaultTolerant : IPlayerGrain, IIndexableGrain<Player5PropertiesNonFaultTolerant>
    {
    }
}
