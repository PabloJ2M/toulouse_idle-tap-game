using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Authentication.Samples
{
    using PlayerAccounts;
    
    public class AuthSignInButton : MonoBehaviour
    {
        [SerializeField] private ProviderType provider;
        
        private void Awake() => GetComponent<Button>().onClick.AddListener(OnClick);
        private void OnClick() => AuthController.Instance?.SignIn(provider);
    }
}