using UnityEngine;

namespace Unity.Services.Authentication
{
    public class AuthenticationManager : SingletonBasic<AuthenticationManager>
    {
        private static AuthResult _result;
        
        public void SignIn() => _ = SignInAsync();
        public void SignIn(AuthProviderType type) => _ = SignInAsync(type);
        
        private static async Awaitable SignInAsync()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            await SignInAsync(AuthProviderType.GooglePlayGames);
            #elif UNITY_IOS && !UNITY_EDITOR
            await SignInAsync(AuthProviderType.AppleGameCenter);
            #else
            await SignInAsync(AuthProviderType.UnityPlayerAccounts);
            #endif
        }
        private static async Awaitable SignInAsync(AuthProviderType type)
        {
            AuthResult result = await AuthProviderRegistry.SignInAsync(type);
            if (result.Success) return;
            
            Debug.LogWarning($"Automatic Login failed ({result.Code}).");
            await AuthenticationService.Instance.SignInAnonymouslyAsync().AuthResponse();
        }
    }
}