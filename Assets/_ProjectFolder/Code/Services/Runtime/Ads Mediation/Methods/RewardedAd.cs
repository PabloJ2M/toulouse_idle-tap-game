using System;

namespace Unity.Services.LevelPlay
{
    public class RewardedAd : AdBehaviour
    {
        private LevelPlayRewardedAd _rewardedAd;
        private event Action<LevelPlayReward> OnRewarded;
        private event Action OnClosed;
        
        public static RewardedAd Instance { get; private set; }
        protected override bool IsReady => _rewardedAd != null && _rewardedAd.IsAdReady();
        
        private void Awake() => Instance = this;
        
        public override void CreateAd()
        {
            _rewardedAd = new(ID);
            
            _rewardedAd.OnAdLoaded += OnAdLoaded;
            _rewardedAd.OnAdLoadFailed += OnAdLoadedFailed;
            
            _rewardedAd.OnAdDisplayed += OnAdDisplayed;
            _rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
            
            _rewardedAd.OnAdInfoChanged += OnAdInfoChanged;
            _rewardedAd.OnAdClicked += OnAdClicked;
            _rewardedAd.OnAdClosed += OnAdClosed;
            _rewardedAd.OnAdRewarded += OnAdRewarded;

            OnLoadAd();
        }

        protected override void OnLoadAd() => _rewardedAd?.LoadAd();
        protected override void OnShowAd() => _rewardedAd?.ShowAd();
        
        protected override void OnAdClosed(LevelPlayAdInfo info)
        {
            OnClosed?.Invoke();
            OnClosed = null;
            OnRewarded = null;
            base.OnAdClosed(info);
        }
        
        private void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward) => OnRewarded?.Invoke(reward);

        public bool TryDisplayAd(Action<LevelPlayReward> onRewarded, Action onClosed = null)
        {
            OnRewarded = onRewarded;
            OnClosed = onClosed;
            
            return TryDisplayAd();
        }
    }
}