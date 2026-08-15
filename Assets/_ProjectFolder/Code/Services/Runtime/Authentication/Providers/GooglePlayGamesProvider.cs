using System;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public class GooglePlayGamesProvider : IAuthProvider
    {
        public ProviderType Type => ProviderType.GooglePlayGames;
        
        public Awaitable SignInAsync() => SignInInternalAsync(false);
        public Awaitable LinkAsync() => SignInInternalAsync(true);

        private async Awaitable SignInInternalAsync(bool link)
        {
            #if UNITY_ANDROID
            
            #else
            throw new PlatformNotSupportedException("Google Play Games is only supported on Android.");
            #endif
        }
    }
}