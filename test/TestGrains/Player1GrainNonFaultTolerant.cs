using System;
using Orleans.Providers;
using UnitTests.GrainInterfaces;

namespace UnitTests.Grains
{
    [Serializable]
    public class Player1GrainStateNonFaultTolerant : Player1PropertiesNonFaultTolerant, PlayerState
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// A simple grain that represent a player in a game
    /// </summary>
    [StorageProvider(ProviderName = "MemoryStore")]
    public class Player1GrainNonFaultTolerant : PlayerGrainNonFaultTolerant<Player1GrainStateNonFaultTolerant, Player1PropertiesNonFaultTolerant>, IPlayer1GrainNonFaultTolerant
    {
    }
}
