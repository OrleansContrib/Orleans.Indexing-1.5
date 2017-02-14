using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player4Properties : PlayerProperties
    {
        public int Score { get; set; }

        [Index(typeof(AHashIndexPartitionedPerKey<string, IPlayer4Grain>)/*, IsEager: false*/, IsUnique: true)]
        public string Location { get; set; }
    }

    public interface IPlayer4Grain : IPlayerGrain, IIndexableGrain<Player4Properties>
    {
    }
}
