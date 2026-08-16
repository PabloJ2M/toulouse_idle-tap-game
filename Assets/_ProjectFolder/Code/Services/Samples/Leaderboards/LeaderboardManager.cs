using UnityEngine;

namespace Unity.Services.Leaderboards.Samples
{
    using Models;
    
    public class LeaderboardManager : LeaderboardsModule
    {
        public static LeaderboardManager Instance { get; private set; }

        private void Awake() => Instance = this;

        public async Awaitable<LeaderboardScoresPage> GetScoresAsync(LeaderboardID id, GetScoresOptions options) =>
            await Service.GetScore(id, options);
    }
}