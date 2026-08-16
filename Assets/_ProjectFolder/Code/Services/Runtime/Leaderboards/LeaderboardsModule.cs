using UnityEngine;

namespace Unity.Services.Leaderboards
{
    using Core;
    using Models;
    
    public abstract class LeaderboardsModule : MonoBehaviour, IServiceModule, IServiceAuthModule
    {
        protected ILeaderboardsService Service;

        public virtual void OnInitialized() => Service = LeaderboardsService.Instance;
        public virtual void OnUserSignedIn(string playerID) { }
        public virtual void OnUserSignedOut() { }
        
        public Awaitable<LeaderboardEntry> AddPlayerScoreAsync(LeaderboardID type, double score) => 
            ServicesStatus.IsSignedIn ? Service.AddPlayerScore(type, score) : null;

        public Awaitable<LeaderboardEntry> GetPlayerScoreAsync(LeaderboardID type) =>
            ServicesStatus.IsSignedIn ? Service?.GetPlayerScore(type) : null;
    }
}