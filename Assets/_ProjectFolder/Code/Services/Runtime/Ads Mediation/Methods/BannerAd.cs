using UnityEngine;

namespace Unity.Services.LevelPlay
{
    public class BannerAd : AdBehaviour
    {
        [SerializeField] private BannerPositionType position = BannerPositionType.BottomCenter;
        
        private LevelPlayBannerAd _bannerAd;

        protected override bool IsReady => _bannerAd != null;
        
        public override void CreateAd()
        {
            var config = new LevelPlayBannerAd.Config.Builder().SetPosition(position.ToBannerPosition()).Build();
            _bannerAd = new(ID, config);
            
            _bannerAd.OnAdLoaded += OnAdLoaded;
            _bannerAd.OnAdLoadFailed += OnAdLoadedFailed;
            
            _bannerAd.OnAdDisplayed += OnAdDisplayed;
            _bannerAd.OnAdDisplayFailed += OnAdDisplayFailed;
            
            _bannerAd.OnAdClicked += OnAdClicked;
            _bannerAd.OnAdExpanded += OnAdExpanded;
            _bannerAd.OnAdCollapsed += OnAdCollapsed;
            _bannerAd.OnAdLeftApplication += OnAdLeftApplication;

            OnLoadAd();
        }
        public void DestroyAd() => _bannerAd?.DestroyAd();
        
        protected override void OnLoadAd() => _bannerAd?.LoadAd();
        protected override void OnShowAd() => _bannerAd?.ShowAd();
        protected override void OnAdLoaded(LevelPlayAdInfo info) => TryDisplayAd();

        private void OnAdCollapsed(LevelPlayAdInfo info) { }
        private void OnAdLeftApplication(LevelPlayAdInfo info) { }
        private void OnAdExpanded(LevelPlayAdInfo info) { }
    }
}