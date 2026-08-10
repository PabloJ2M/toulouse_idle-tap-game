namespace Unity.Notifications
{
    public interface INotification
    {
        void RequestAuthorization();
        void SendNotification(string title, string body, string subTitle, int timeInHours);
        void CancelNotification();
    }
}