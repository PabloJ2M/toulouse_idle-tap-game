namespace Unity.Services.Authentication
{
    public enum AuthResultCode
    {
        Success,
        Cancelled,
        NoConnection,
        InvalidCredentials,
        AlreadyLinked,
        ProviderNotRegistered,
        Unknown
    }
    
    public readonly struct AuthResult
    {
        public readonly bool Success;
        public readonly AuthResultCode Code;
        public readonly string Message;
 
        private AuthResult(bool success, AuthResultCode code, string message)
        {
            Success = success;
            Code = code;
            Message = message;
        }
 
        public static AuthResult Ok() => new AuthResult(true, AuthResultCode.Success, null);
        public static AuthResult Fail(AuthResultCode code, string message = null) => new AuthResult(false, code, message);
    }
}