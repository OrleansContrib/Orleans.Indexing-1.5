using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player4PropertiesNonFaultTolerantLazy : PlayerProperties
    {
        public int Score { get; set; }

        [Index(typeof(AHashIndexPartitionedPerKey<string, IPlayer4GrainNonFaultTolerantLazy>)/*, IsEager: false*/, IsUnique: true)]
        public string Location { get; set; }
    }

    public interface IPlayer4GrainNonFaultTolerantLazy : IPlayerGrain, IIndexableGrain<Player4PropertiesNonFaultTolerantLazy>
    {
    }
}
