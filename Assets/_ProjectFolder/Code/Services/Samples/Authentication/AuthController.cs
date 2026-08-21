using UnityEngine;

namespace Unity.Services.Authentication.Samples
{
    using PlayerAccounts;
    
    public class AuthController : AuthModule
    {
        [SerializeField] private bool debugging = false;

        public override void OnInitialized()
        {
            base.OnInitialized();
            _ = SignInAsync();
        }
        public void SignIn(ProviderType type) => _ = SignInAsync(type);
        
        private async Awaitable SignInAsync()
        {
            if (debugging) {
                await SignInAsync(ProviderType.Anonymous);
                return;
            }
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            await SignInAsync(AuthProviderType.GooglePlayGames);
            #elif UNITY_IOS && !UNITY_EDITOR
            await SignInAsync(AuthProviderType.AppleGameCenter);
            #else
            await SignInAsync(ProviderType.UnityPlayerAccounts);
            #endif

            OnPlayerNameChanged?.Invoke(Service.PlayerName);
        }
        private static async Awaitable SignInAsync(ProviderType type)
        {
            var result = await AuthProviderRegistry.SignInAsync(type);
            if (result.Success) return;
            
            Debug.LogWarning($"Automatic Login failed ({result.Code}).");
            await AuthenticationService.Instance.SignInAnonymouslyAsync().Response();
        }
    }
}