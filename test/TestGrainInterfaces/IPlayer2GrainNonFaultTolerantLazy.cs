using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player2PropertiesNonFaultTolerantLazy : PlayerProperties
    {
        public int Score { get; set; }

        [AIndex(IndexType.HashIndexPartitionedBySilo/*, IsEager: false*/)]
        public string Location { get; set; }
    }

    public interface IPlayer2GrainNonFaultTolerantLazy : IPlayerGrain, IIndexableGrain<Player2PropertiesNonFaultTolerantLazy>
    {
    }
}
