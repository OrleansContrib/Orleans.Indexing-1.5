using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player2Properties : PlayerProperties
    {
        public int Score { get; set; }

        [AIndex(IndexType.HashIndexPartitionedBySilo)]
        public string Location { get; set; }
    }

    public interface IPlayer2Grain : IPlayerGrain, IIndexableGrain<Player2Properties>
    {
    }
}
