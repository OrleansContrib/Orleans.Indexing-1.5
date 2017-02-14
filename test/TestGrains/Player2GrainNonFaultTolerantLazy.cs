using System;
using Orleans.Providers;
using UnitTests.GrainInterfaces;

namespace UnitTests.Grains
{
    [Serializable]
    public class Player2GrainStateNonFaultTolerantLazy : Player2PropertiesNonFaultTolerantLazy, PlayerState
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// A simple grain that represent a player in a game
    /// </summary>
    [StorageProvider(ProviderName = "MemoryStore")]
    public class Player2GrainNonFaultTolerantLazy : PlayerGrainNonFaultTolerant<Player2GrainStateNonFaultTolerantLazy, Player2PropertiesNonFaultTolerantLazy>, IPlayer2GrainNonFaultTolerantLazy
    {
    }
}
