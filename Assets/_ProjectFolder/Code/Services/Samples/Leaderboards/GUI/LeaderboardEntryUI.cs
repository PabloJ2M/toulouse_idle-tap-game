using UnityEngine;
using TMPro;

namespace Unity.Services.Leaderboards.Samples.UI
{
    using Models;
    
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI scoreText;

        private const string RankFormat = "#0";
        private const string ScoreFormat = "0";
        
        public void Setup(LeaderboardEntry entry)
        {
            rankText?.SetText(entry.Rank.ToString(RankFormat));
            nameText?.SetText(entry.PlayerName);
            scoreText?.SetText(entry.Score.ToString(ScoreFormat));
        }
    }
}