using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public static class AuthProviderRegistry
    {
        private static readonly Dictionary<ProviderType, IAuthProvider> Providers = new()
        {
            { ProviderType.Anonymous, new AnonymousProvider() },
            { ProviderType.UnityPlayerAccounts, new UnityPlayerAccountsProvider() },
            { ProviderType.GooglePlayGames, new GooglePlayGamesProvider() },
            { ProviderType.AppleGameCenter, new AppleGameCenterProvider() },
        };
 
        public static async Awaitable<AuthResult> SignInAsync(ProviderType type)
        {
            if (!Providers.TryGetValue(type, out var provider))
                return AuthResult.Fail(AuthResultCode.ProviderNotRegistered, $"Non sign-in provider for {type}");
 
            return await provider.SignInAsync().Response();
        }
        public static async Awaitable<AuthResult> LinkAsync(ProviderType type)
        {
            if (!Providers.TryGetValue(type, out var provider))
                return AuthResult.Fail(AuthResultCode.ProviderNotRegistered, $"Non sign-in provider for {type}");
 
            return await provider.LinkAsync().Response();
        }
    }
}