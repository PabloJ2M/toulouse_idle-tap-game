using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public class AuthSignInButton : MonoBehaviour
    {
        [SerializeField] private AuthProviderType provider;
        
        private void Awake() => GetComponent<Button>().onClick.AddListener(OnClick);
        private void OnClick() => AuthenticationManager.Instance?.SignIn(provider);
    }
}