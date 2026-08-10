using System;
using UnityEngine;

namespace Unity.Services.Authentication
{
    public class AppleGameCenterProvider : IAuthProvider
    {
        public AuthProviderType Type => AuthProviderType.AppleGameCenter;

        public Awaitable SignInAsync() => SignInInternalAsync(false);
        public Awaitable LinkAsync() => SignInInternalAsync(true);

        private async Awaitable SignInInternalAsync(bool link)
        {
            #if UNITY_IOS
            
            #else
            throw new PlatformNotSupportedException("Game Center is only supported in IOs and MacOS");
            #endif
        }
    }
}