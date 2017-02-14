using System;
using Orleans.Providers;
using UnitTests.GrainInterfaces;

namespace UnitTests.Grains
{
    [Serializable]
    public class Player2GrainState : Player2Properties, PlayerState
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// A simple grain that represent a player in a game
    /// </summary>
    [StorageProvider(ProviderName = "GrainStore")]
    public class Player2Grain : PlayerGrain<Player2GrainState, Player2Properties>, IPlayer2Grain
    {
    }
}
