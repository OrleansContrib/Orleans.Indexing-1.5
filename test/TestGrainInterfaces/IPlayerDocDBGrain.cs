using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class PlayerDocDBProperties : PlayerProperties
    {
        [Index]
        public int Score { get; set; }
        
        [DSMIndex]
        public string Location { get; set; }
    }

    public interface IPlayerDocDBGrain : IPlayerGrain, IIndexableGrain<PlayerDocDBProperties>
    {
    }
}
