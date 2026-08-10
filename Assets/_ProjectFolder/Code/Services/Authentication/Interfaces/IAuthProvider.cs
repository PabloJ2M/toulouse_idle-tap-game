using UnityEngine;

namespace Unity.Services.Authentication
{
    public interface IAuthProvider
    {
        AuthProviderType Type { get; }
        
        Awaitable SignInAsync();
        Awaitable LinkAsync();
    }
}