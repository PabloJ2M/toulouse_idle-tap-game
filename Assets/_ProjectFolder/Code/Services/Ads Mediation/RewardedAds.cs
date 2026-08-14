using System;

namespace Unity.Services.LevelPlay
{
    public class RewardedAds : AdsBehaviour
    {
        private LevelPlayRewardedAd _rewardedAd;
        public AdsType Type => AdsType.Rewarded;
        
        private Action<LevelPlayReward> onAdRewarded;
        
        protected override void OnCreateAd()
        {
            _rewardedAd = new LevelPlayRewardedAd(ID);
            
            _rewardedAd.OnAdLoaded += OnAdLoaded;
            _rewardedAd.OnAdLoadFailed += OnAdLoadedFailed;
            
            _rewardedAd.OnAdDisplayed += OnAdDisplayed;
            _rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
            
            _rewardedAd.OnAdClicked += OnAdClicked;
            _rewardedAd.OnAdClosed += OnAdClosed;
            _rewardedAd.OnAdRewarded += OnAdRewarded;
        }
        protected override void OnDisplayAd() => _rewardedAd?.Dispose();
        
        private void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward) => onAdRewarded?.Invoke(reward);

        public void DisplayAd(Action<LevelPlayReward> onRewarded)
        {
            onAdRewarded = onRewarded;
            DisplayAd();
        }
    }
}