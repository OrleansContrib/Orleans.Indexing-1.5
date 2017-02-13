using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player1PropertiesNonFaultTolerant : PlayerProperties
    {
        public int Score { get; set; }

        [AIndex(IsEager : true)]
        public string Location { get; set; }
    }

    public interface IPlayer1GrainNonFaultTolerant : IPlayerGrain, IIndexableGrain<Player1PropertiesNonFaultTolerant>
    {
    }
}
