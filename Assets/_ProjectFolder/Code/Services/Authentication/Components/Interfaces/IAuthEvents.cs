namespace Unity.Services.Authentication.Components
{
    public interface IAuthEvents
    {
        void OnSignIn();
        void OnSignOut();
    }
}