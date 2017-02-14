using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{
    [Serializable]
    public class PlayerChain1Properties : PlayerProperties
    {
        [Index]
        public int Score { get; set; }
        
        [AIndex(IndexType.HashIndexSingleBucket,true,5)]
        public string Location { get; set; }
    }

    public interface IPlayerChain1Grain : IPlayerGrain, IIndexableGrain<PlayerChain1Properties>
    {
    }
}
