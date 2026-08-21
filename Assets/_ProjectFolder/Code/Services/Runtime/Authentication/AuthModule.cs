using System;
using UnityEngine;

namespace Unity.Services.Authentication
{
    using Core;
    using PlayerAccounts;
    
    public abstract class AuthModule : ServicesStatic<AuthModule>, IServiceModule
    {
        protected static IAuthenticationService Service { get; private set; }
        
        public static Action<string> OnPlayerNameChanged;

        public virtual void OnInitialized() => Service = AuthenticationService.Instance;

        public static async Awaitable ChangePlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            
            var newName = await Service.UpdateName(name);
            OnPlayerNameChanged?.Invoke(newName);
        }
    }
}