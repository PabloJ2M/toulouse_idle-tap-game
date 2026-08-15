using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public interface IAuthProvider
    {
        ProviderType Type { get; }
        
        Awaitable SignInAsync();
        Awaitable LinkAsync();
    }
}