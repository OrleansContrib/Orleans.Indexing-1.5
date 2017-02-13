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
        
        public string Location { get; set; }
    }

    public interface IPlayer1GrainNonFaultTolerant : IPlayerGrain
    {
    }
}
