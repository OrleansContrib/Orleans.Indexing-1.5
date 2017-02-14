using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class Player1PropertiesNonFaultTolerantLazy : PlayerProperties
    {
        public int Score { get; set; }

        [AIndex/*(IsEager : false)*/]
        public string Location { get; set; }
    }

    public interface IPlayer1GrainNonFaultTolerantLazy : IPlayerGrain, IIndexableGrain<Player1PropertiesNonFaultTolerantLazy>
    {
    }
}
