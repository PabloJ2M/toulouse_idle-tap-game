using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public class UnityPlayerAccountsProvider : IAuthProvider
    {
        public ProviderType Type => ProviderType.UnityPlayerAccounts;
        
        public Awaitable SignInAsync() => SignInInternalAsync(false);
        public Awaitable LinkAsync() => SignInInternalAsync(true);

        private static async Awaitable SignInInternalAsync(bool link)
        {
            if (!PlayerAccountService.Instance.IsSignedIn)
                await PlayerAccountService.Instance.StartSignInAsync();
 
            string accessToken = PlayerAccountService.Instance.AccessToken;
 
            if (link)
                await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);
            else
                await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
        }
    }
}