using System;
using Orleans.Providers;
using UnitTests.GrainInterfaces;

namespace UnitTests.Grains
{
    [Serializable]
    public class PlayerDocDBGrainState : PlayerDocDBProperties, PlayerState
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// A simple grain that represent a player in a game
    /// </summary>
    [StorageProvider(ProviderName = "DocumentDBStore")]
    public class PlayerDocDBGrain : PlayerGrainNonFaultTolerant<PlayerDocDBGrainState, PlayerDocDBProperties>, IPlayerDocDBGrain
    {
    }
}
