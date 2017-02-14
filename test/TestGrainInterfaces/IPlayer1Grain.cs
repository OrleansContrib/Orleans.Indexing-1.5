using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player1Properties : PlayerProperties
    {
        public int Score { get; set; }

        [IIndex]
        public string Location { get; set; }
    }

    public interface IPlayer1Grain : IPlayerGrain, IIndexableGrain<Player1Properties>
    {
    }
}
