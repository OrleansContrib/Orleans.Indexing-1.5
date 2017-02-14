using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player5PropertiesNonFaultTolerantLazy : PlayerProperties
    {
        [Index(typeof(AHashIndexSingleBucket<string, IPlayer5GrainNonFaultTolerantLazy>)/*, IsEager: false*/, IsUnique: true)]
        public int Score { get; set; }

        [Index(typeof(AHashIndexPartitionedPerKey<string, IPlayer5GrainNonFaultTolerantLazy>)/*, IsEager: false*/, IsUnique: true)]
        public string Location { get; set; }
    }

    public interface IPlayer5GrainNonFaultTolerantLazy : IPlayerGrain, IIndexableGrain<Player5PropertiesNonFaultTolerantLazy>
    {
    }
}
