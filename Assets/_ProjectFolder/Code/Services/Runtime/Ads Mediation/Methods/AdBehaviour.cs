using UnityEngine;

namespace Unity.Services.LevelPlay
{
    public abstract class AdBehaviour : MonoBehaviour
    {
        [SerializeField] private string androidId;
        [SerializeField] private string appleId;

        protected string ID
        {
            get
            {
                #if UNITY_ANDROID
                return androidId;
                #elif UNITY_IOS
                return appleId;
                #else
                return androidId;
                #endif
            }
        }
        
        protected abstract bool IsReady { get; }
        
        public abstract void CreateAd();
        protected abstract void OnLoadAd();
        protected abstract void OnShowAd();
        
        public bool TryDisplayAd()
        {
            if (!IsReady) return false;
            OnShowAd();
            return true;
        }
        
        protected virtual void OnAdLoaded(LevelPlayAdInfo info) { }
        protected virtual void OnAdLoadedFailed(LevelPlayAdError error) => OnLoadAd();
        protected virtual void OnAdDisplayed(LevelPlayAdInfo info) { }
        protected virtual void OnAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error) { }
        protected virtual void OnAdInfoChanged(LevelPlayAdInfo info) { }
        protected virtual void OnAdClicked(LevelPlayAdInfo info) { }
        protected virtual void OnAdClosed(LevelPlayAdInfo info) => OnLoadAd();
    }
}