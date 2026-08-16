using UnityEngine;
using UnityEngine.Events;

namespace Unity.Services.Leaderboards.Samples.UI
{
    using Models;
    
    public class LeaderboardScoresPageUI : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        [SerializeField] private LeaderboardEntryUI prefab;
        
        [Header("Leaderboard")]
        [SerializeField] private LeaderboardID leaderboardId;
        [SerializeField] private int limit;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<bool> onLoadingScores;
        [SerializeField] private UnityEvent<bool> onError;

        private readonly GetScoresOptions _operation = new();
        private int _offset;

        private void Awake() => _operation.Limit = limit;
        private void OnEnable() => UpdateTable();
        private void OnValidate() => UpdateTable();
        private void UpdateTable() => _ = LoadTable();
        
        private async Awaitable LoadTable()
        {
            if (!Application.isPlaying) return;
            onLoadingScores.Invoke(true);
            onError.Invoke(false);
            
            var page = await LeaderboardManager.Instance.GetScoresAsync(leaderboardId, _operation);
            onLoadingScores.Invoke(false);
            Clear();
            
            if (page == null) {
                onError.Invoke(true);
                return;
            }

            Build(page);
        }

        private void Build(LeaderboardScoresPage page)
        {
            foreach (var entry in page.Results)
                Instantiate(prefab, parent).Setup(entry);
        }
        private void Clear()
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }
        
        public void ChangeLeaderboard(LeaderboardID newId)
        {
            _operation.Offset = _offset = 0;
            leaderboardId = newId;
            UpdateTable();
        }
        public void Previous()
        {
            _offset -= limit;
            _operation.Offset = Mathf.Max(_offset, 0);
            UpdateTable();
        }
        public void Next()
        {
            _offset += limit;
            _operation.Offset = _offset;
            UpdateTable();
        }
    }
}