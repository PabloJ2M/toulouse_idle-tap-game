using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Friends.Samples.UI
{
    public class FriendsEntryUIRequestOutgoing : FriendsEntryUI
    {
        [SerializeField] private Button cancelButton;

        private void OnEnable() => cancelButton.onClick.AddListener(Cancel);
        private void OnDisable() => cancelButton.onClick.RemoveListener(Cancel);
        private void Cancel() => FriendsManager.CancelRequest(ID);
    }
}