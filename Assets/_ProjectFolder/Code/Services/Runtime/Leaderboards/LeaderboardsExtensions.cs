using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Leaderboards
{
    using Models;
    using Exceptions;
    
    public static class LeaderboardsExtensions
    {
        private static async Awaitable<T> Response<T>(this Task<T> action) where T : class
        {
            try { return await action; }
            catch (LeaderboardsValidationException e) { Debug.LogError(e); }
            catch (LeaderboardsRateLimitedException e) { Debug.LogError(e); }
            catch (LeaderboardsException e) { Debug.LogError(e); }
            return null;
        }
        private static async Awaitable<T> Response<T>(this Awaitable<T> action) where T : class
        {
            try { return await action; }
            catch (LeaderboardsValidationException e) { Debug.LogError(e); }
            catch (LeaderboardsRateLimitedException e) { Debug.LogError(e); }
            catch (LeaderboardsException e) { Debug.LogError(e); }
            return null;
        }
        
        public static async Awaitable<LeaderboardScoresPage> GetScore(this ILeaderboardsService service, LeaderboardID type) =>
            await service.GetScoresAsync(type.ToString()).Response();
        
        public static async Awaitable<LeaderboardScoresPage> GetScore(this ILeaderboardsService service, LeaderboardID type, int limit) =>
            await service.GetScore(type, new GetScoresOptions { Limit = limit });
        
        public static async Awaitable<LeaderboardScoresPage> GetScore(this ILeaderboardsService service, LeaderboardID type, GetScoresOptions options) =>
            await service.GetScoresAsync(type.ToString(), options).Response();
        
        public static async Awaitable<LeaderboardEntry> GetPlayerScore(this ILeaderboardsService service, LeaderboardID type) =>
            await service.GetPlayerScoreAsync(type.ToString()).Response();
        
        public static async Awaitable<LeaderboardScores> GetPlayerRange(this ILeaderboardsService service, LeaderboardID type) =>
            await service.GetPlayerRangeAsync(type.ToString()).Response();
        
        public static async Awaitable<LeaderboardEntry> AddPlayerScore(this ILeaderboardsService service, LeaderboardID type, double amount) =>
            await service.AddPlayerScoreAsync(type.ToString(), amount).Response();
    }
}