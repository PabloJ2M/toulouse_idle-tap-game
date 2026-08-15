using UnityEngine;

namespace Unity.Services.Core
{
    using Authentication;
    using Authentication.Components;
    
    [RequireComponent(typeof(PlayerAuthentication))]
    [AddComponentMenu("Services/Player Authentication Bridge")]
    public class PlayerAuthenticationBridge : MonoBehaviour
    {
        private PlayerAuthentication _playerAuthentication;
        private IServiceAuthModule[] _services;
        
        private void Awake()
        {
            _playerAuthentication = GetComponent<PlayerAuthentication>();
            _services = GetComponentsInChildren<IServiceAuthModule>();
        }
        
        private void OnEnable()
        {
            _playerAuthentication.Events.SignedIn.AddListener(OnSignInCompleted);
            _playerAuthentication.Events.SignedOut.AddListener(OnSignOutCompleted);
        }
        private void OnDisable()
        {
            _playerAuthentication.Events.SignedIn.RemoveListener(OnSignInCompleted);
            _playerAuthentication.Events.SignedOut.RemoveListener(OnSignOutCompleted);
        }

        private void OnSignInCompleted()
        {
            var playerId = AuthenticationService.Instance.PlayerId;
            ServicesStatus.IsSignedIn = true;
            
            foreach (var service in _services)
                service.OnUserSignedIn(playerId);
        }
        private void OnSignOutCompleted()
        {
            ServicesStatus.IsSignedIn = false;
            
            foreach (var service in _services)
                service.OnUserSignedOut();
        }
    }
}