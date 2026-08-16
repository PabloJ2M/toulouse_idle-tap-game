using UnityEngine;

namespace Unity.Services.Leaderboards.Samples
{
    public class LeaderboardTrigger : MonoBehaviour
    {
        [SerializeField] private LeaderboardID id;
        
        public void AddScore(double score) =>
            _ = LeaderboardManager.Instance.AddPlayerScoreAsync(id, score);
    }
}