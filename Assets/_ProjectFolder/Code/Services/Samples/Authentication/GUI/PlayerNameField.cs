using UnityEngine;
using TMPro;

namespace Unity.Services.Authentication.Samples.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public class PlayerNameField : MonoBehaviour
    {
        private TMP_InputField _inputField;
        
        private void Awake() => _inputField = GetComponent<TMP_InputField>();
        private void OnEnable() => AuthModule.OnPlayerNameChanged += _inputField.SetTextWithoutNotify;
        private void OnDisable() => AuthModule.OnPlayerNameChanged -= _inputField.SetTextWithoutNotify;
        
        public void ChangeName() => _ = AuthModule.ChangePlayerName(_inputField.text);
    }
}