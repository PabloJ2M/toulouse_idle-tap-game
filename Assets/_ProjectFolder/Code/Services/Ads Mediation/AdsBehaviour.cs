using UnityEngine;

namespace Unity.Services.LevelPlay
{
    public abstract class AdsBehaviour : MonoBehaviour
    {
        [SerializeField] private string androidId;
        [SerializeField] private string iosId;

        protected string ID
        {
            get
            {
                #if UNITY_ANDROID
                return androidId;
                #elif UNITY_IOS
                return iosId;
                #else
                return androidId;
                #endif
            }
        }
        
        protected virtual void Start() => OnCreateAd();
        protected abstract void OnCreateAd();
        protected abstract void OnDisplayAd();
        
        public void DisplayAd()
        {
            OnDisplayAd();
            OnCreateAd();
        }
        
        protected virtual void OnAdLoaded(LevelPlayAdInfo info) { }
        protected virtual void OnAdLoadedFailed(LevelPlayAdError error) { }
        protected virtual void OnAdDisplayed(LevelPlayAdInfo info) { }
        protected virtual void OnAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error) { }
        
        protected virtual void OnAdClicked(LevelPlayAdInfo info) { }
        protected virtual void OnAdClosed(LevelPlayAdInfo info) { }
    }
}