using System;

namespace Unity.Services.Authentication
{
    public sealed class AuthCancelledException : Exception
    {
        public AuthCancelledException(string message) : base(message) { }
    }
}