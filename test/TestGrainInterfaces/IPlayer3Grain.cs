using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player3Properties : PlayerProperties
    {
        public int Score { get; set; }

        [IIndex(IndexType.HashIndexPartitionedByKeyHash)]
        public string Location { get; set; }
    }

    public interface IPlayer3Grain : IPlayerGrain, IIndexableGrain<Player3Properties>
    {
    }
}
