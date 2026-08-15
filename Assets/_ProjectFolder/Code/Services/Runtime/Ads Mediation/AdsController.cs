using UnityEngine;

namespace Unity.Services.LevelPlay
{
    public class AdsController : MonoBehaviour
    {
        [SerializeField] private string androidAppKey;
        [SerializeField] private string appleAppKey;
        [SerializeField] private bool isProduction;
        
        private string AppKey
        {
            get
            {
                #if UNITY_ANDROID
                return androidAppKey;
                #elif UNITY_IOS
                return appleAppKey;
                #else
                return string.Empty;
                #endif
            }
        }

        private void Start()
        {
            if (!isProduction)
                LevelPlay.ValidateIntegration();
            
            LevelPlay.OnInitFailed += OnInitFailed;
            LevelPlay.OnInitSuccess += OnInitSuccess;
            LevelPlay.Init(AppKey);
        }
        
        private void OnInitFailed(LevelPlayInitError error) { }
        private void OnInitSuccess(LevelPlayConfiguration config)
        {
            var ads = GetComponentsInChildren<AdBehaviour>();
            
            foreach (var ad in ads)
                ad.CreateAd();
        }
    }
}