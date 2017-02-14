using System;
using Orleans.Providers;
using UnitTests.GrainInterfaces;

namespace UnitTests.Grains
{
    [Serializable]
    public class Player2GrainStateNonFaultTolerant : Player2PropertiesNonFaultTolerant, PlayerState
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// A simple grain that represent a player in a game
    /// </summary>
    [StorageProvider(ProviderName = "MemoryStore")]
    public class Player2GrainNonFaultTolerant : PlayerGrainNonFaultTolerant<Player2GrainStateNonFaultTolerant, Player2PropertiesNonFaultTolerant>, IPlayer2GrainNonFaultTolerant
    {
    }
}
