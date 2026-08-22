using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    using Core;
    
    public static class AuthExtensions
    {
        public static async Awaitable<AuthResult> Response(this Task action)
        {
            try { await action; return AuthResult.Ok(); }
            catch (AuthCancelledException ex) { return AuthResult.Fail(AuthResultCode.Cancelled, ex.Message); }
            catch (AuthenticationException ex) { Debug.LogError(ex); return AuthResult.Fail(MapAuthCode(ex), ex.Message); }
            catch (PlayerAccountsException ex) { Debug.LogError(ex); return AuthResult.Fail(AuthResultCode.Cancelled, ex.Message); }
            catch (RequestFailedException ex) { Debug.LogError(ex); return AuthResult.Fail(AuthResultCode.NoConnection, ex.Message); }
        }
        public static async Awaitable<AuthResult> Response(this Awaitable action)
        {
            try { await action; return AuthResult.Ok(); }
            catch (AuthCancelledException ex) { return AuthResult.Fail(AuthResultCode.Cancelled, ex.Message); }
            catch (AuthenticationException ex) { Debug.LogError(ex); return AuthResult.Fail(MapAuthCode(ex), ex.Message); }
            catch (PlayerAccountsException ex) { Debug.LogError(ex); return AuthResult.Fail(AuthResultCode.Cancelled, ex.Message); }
            catch (RequestFailedException ex) { Debug.LogError(ex); return AuthResult.Fail(AuthResultCode.NoConnection, ex.Message); }
        }

        private static AuthResultCode MapAuthCode(AuthenticationException ex) => ex.ErrorCode switch
        {
            var code when code == AuthenticationErrorCodes.AccountAlreadyLinked => AuthResultCode.AlreadyLinked,
            var code when code == AuthenticationErrorCodes.InvalidParameters => AuthResultCode.InvalidCredentials,
            _ => AuthResultCode.Unknown
        };
        
        public static async Awaitable<string> UpdateName(this IAuthenticationService service, string newName) =>
            await service.UpdatePlayerNameAsync(newName);
    }
}