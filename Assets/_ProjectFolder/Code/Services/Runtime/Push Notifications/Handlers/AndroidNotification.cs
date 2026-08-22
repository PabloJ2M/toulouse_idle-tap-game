using UnityEngine;
using UnityEngine.Android;

namespace Unity.Notifications
{
    using Android;
    
    public class AndroidNotification : MonoBehaviour, INotification
    {
        public void RequestAuthorization()
        {
            
        }
        public void SendNotification(string title, string body, string subTitle, int timeInHours)
        {
            
        }
        public void CancelNotification()
        {
            
        }
    }
}