using System.Collections;
using UnityEngine;

namespace Unity.Notifications
{
    using iOS;
    
    public class IOSNotification : MonoBehaviour, INotification
    {
        public void RequestAuthorization() => StartCoroutine(RequestAuthorizationRoutine());
        
        public void SendNotification(string title, string body, string subTitle, int timeInHours)
        {
            
        }
        public void CancelNotification()
        {
            
        }
        
        private IEnumerator RequestAuthorizationRoutine()
        {
            yield break;
        }
    }
}