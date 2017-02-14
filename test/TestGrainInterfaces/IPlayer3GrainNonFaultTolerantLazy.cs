using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player3PropertiesNonFaultTolerantLazy : PlayerProperties
    {
        public int Score { get; set; }

        [AIndex(IndexType.HashIndexPartitionedByKeyHash/*, IsEager: false*/)]
        public string Location { get; set; }
    }

    public interface IPlayer3GrainNonFaultTolerantLazy : IPlayerGrain, IIndexableGrain<Player3PropertiesNonFaultTolerantLazy>
    {
    }
}
