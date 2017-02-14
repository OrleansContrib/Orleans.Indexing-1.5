using System;
using Orleans.Providers;
using UnitTests.GrainInterfaces;

namespace UnitTests.Grains
{
    [Serializable]
    public class Player1GrainState : Player1Properties, PlayerState
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// A simple grain that represent a player in a game
    /// </summary>
    [StorageProvider(ProviderName = "GrainStore")]
    public class Player1Grain : PlayerGrain<Player1GrainState, Player1Properties>, IPlayer1Grain
    {
    }
}
