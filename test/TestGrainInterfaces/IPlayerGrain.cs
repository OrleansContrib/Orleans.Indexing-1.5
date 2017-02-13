using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Indexing;

namespace UnitTests.GrainInterfaces
{

    public interface PlayerProperties
    {
        int Score { get; set; }
        
        string Location { get; set; }
    }

    public interface PlayerState : PlayerProperties
    {
        string Email { get; set; }
    }

    public interface IPlayerGrain : IGrainWithIntegerKey
    {
        Task<string> GetEmail();
        Task<string> GetLocation();
        Task<int> GetScore();

        Task SetEmail(string email);
        Task SetLocation(string location);
        Task SetScore(int score);

        Task Deactivate();
    }
}
