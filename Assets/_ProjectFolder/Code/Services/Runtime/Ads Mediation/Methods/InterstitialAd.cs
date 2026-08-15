namespace Unity.Services.LevelPlay
{
    public class InterstitialAd : AdBehaviour
    {
        private LevelPlayInterstitialAd _interstitialAd;

        public static InterstitialAd Instance { get; private set; }
        protected override bool IsReady => _interstitialAd != null && _interstitialAd.IsAdReady();

        private void Awake() => Instance = this;

        public override void CreateAd()
        {
            _interstitialAd = new(ID);
            
            _interstitialAd.OnAdLoaded += OnAdLoaded;
            _interstitialAd.OnAdLoadFailed += OnAdLoadedFailed;
            
            _interstitialAd.OnAdDisplayed += OnAdDisplayed;
            _interstitialAd.OnAdDisplayFailed += OnAdDisplayFailed;
            
            _interstitialAd.OnAdInfoChanged += OnAdInfoChanged;
            _interstitialAd.OnAdClicked += OnAdClicked;
            _interstitialAd.OnAdClosed += OnAdClosed;
            
            OnLoadAd();
        }

        protected override void OnLoadAd() => _interstitialAd?.LoadAd();
        protected override void OnShowAd() => _interstitialAd?.ShowAd();
    }
}