using System;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public sealed class AuthCancelledException : Exception
    {
        public AuthCancelledException(string message) : base(message) { }
    }
}