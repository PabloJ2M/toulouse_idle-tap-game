using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public static class AuthProviderRegistry
    {
        private static readonly Dictionary<AuthProviderType, IAuthProvider> Providers = new()
        {
            { AuthProviderType.Anonymous, new AnonymousProvider() },
            { AuthProviderType.UnityPlayerAccounts, new UnityPlayerAccountsProvider() },
            { AuthProviderType.GooglePlayGames, new GooglePlayGamesProvider() },
            { AuthProviderType.AppleGameCenter, new AppleGameCenterProvider() },
        };
 
        public static async Awaitable<AuthResult> SignInAsync(AuthProviderType type)
        {
            if (!Providers.TryGetValue(type, out var provider))
                return AuthResult.Fail(AuthResultCode.ProviderNotRegistered, $"Non sign-in provider for {type}");
 
            return await provider.SignInAsync().Response();
        }
        public static async Awaitable<AuthResult> LinkAsync(AuthProviderType type)
        {
            if (!Providers.TryGetValue(type, out var provider))
                return AuthResult.Fail(AuthResultCode.ProviderNotRegistered, $"Non sign-in provider for {type}");
 
            return await provider.LinkAsync().Response();
        }
    }
}