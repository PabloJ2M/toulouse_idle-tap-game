using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public class AnonymousProvider : IAuthProvider
    {
        public ProviderType Type => ProviderType.AppleGameCenter;

        public Awaitable SignInAsync() => SignInInternalAsync();
        public Awaitable LinkAsync() => SignInInternalAsync();
        
        private static async Awaitable SignInInternalAsync()
        {
            #if UNITY_EDITOR
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            #endif
        }
    }
}