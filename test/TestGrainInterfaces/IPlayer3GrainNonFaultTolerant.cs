using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player3PropertiesNonFaultTolerant : PlayerProperties
    {
        public int Score { get; set; }

        [AIndex(IndexType.HashIndexPartitionedByKeyHash, IsEager: true)]
        public string Location { get; set; }
    }

    public interface IPlayer3GrainNonFaultTolerant : IPlayerGrain, IIndexableGrain<Player3PropertiesNonFaultTolerant>
    {
    }
}
