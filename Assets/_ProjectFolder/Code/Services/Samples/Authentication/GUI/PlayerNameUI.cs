using UnityEngine;
using TMPro;

namespace Unity.Services.Authentication.Samples.UI
{
    public class PlayerNameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;

        private void OnEnable() => AuthModule.OnPlayerNameChanged += nameText.SetText;
        private void OnDisable() => AuthModule.OnPlayerNameChanged -= nameText.SetText;
    }
}