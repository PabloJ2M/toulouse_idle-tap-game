using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public interface IAuthProvider
    {
        AuthProviderType Type { get; }
        
        Awaitable SignInAsync();
        Awaitable LinkAsync();
    }
}