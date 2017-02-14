using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player4PropertiesNonFaultTolerant : PlayerProperties
    {
        public int Score { get; set; }

        [Index(typeof(AHashIndexPartitionedPerKey<string, IPlayer4GrainNonFaultTolerant>), IsEager: true, IsUnique: true)]
        public string Location { get; set; }
    }

    public interface IPlayer4GrainNonFaultTolerant : IPlayerGrain, IIndexableGrain<Player4PropertiesNonFaultTolerant>
    {
    }
}
