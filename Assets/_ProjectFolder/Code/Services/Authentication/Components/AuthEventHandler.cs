using UnityEngine;

namespace Unity.Services.Authentication.Components
{
    [RequireComponent(typeof(IAuthEvents))]
    public class AuthEventHandler : MonoBehaviour
    {
        private PlayerAuthentication _playerAuth;
        private IAuthEvents _events;
        
        private void Awake()
        {
            _events = GetComponent<IAuthEvents>();
            _playerAuth = GetComponentInParent<PlayerAuthentication>();
        }
        private void Start()
        {
            _playerAuth.Events.SignedIn.AddListener(_events.OnSignIn);
            _playerAuth.Events.SignedOut.AddListener(_events.OnSignOut);
            
            if (_playerAuth.AuthenticationService.IsSignedIn)
                _events.OnSignIn();
        }
        private void OnDestroy()
        {
            if (!_playerAuth) return;
            _playerAuth.Events.SignedIn.RemoveListener(_events.OnSignIn);
            _playerAuth.Events.SignedOut.RemoveListener(_events.OnSignOut);
        }
    }
}