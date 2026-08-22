namespace Unity.Services.Core
{
    public interface IServiceAuthModule
    {
        void OnUserSignedIn(string playerID);
        void OnUserSignedOut();
    }
}