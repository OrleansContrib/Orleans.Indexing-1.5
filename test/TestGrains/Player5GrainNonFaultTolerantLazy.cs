using System;
using Orleans.Providers;
using UnitTests.GrainInterfaces;

namespace UnitTests.Grains
{
    [Serializable]
    public class Player5GrainStateNonFaultTolerantLazy : Player5PropertiesNonFaultTolerantLazy, PlayerState
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// A simple grain that represent a player in a game
    /// </summary>
    [StorageProvider(ProviderName = "MemoryStore")]
    public class Player5GrainNonFaultTolerantLazy : PlayerGrainNonFaultTolerant<Player5GrainStateNonFaultTolerantLazy, Player5PropertiesNonFaultTolerantLazy>, IPlayer5GrainNonFaultTolerantLazy
    {
    }
}
