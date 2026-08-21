using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Friends.Samples.UI
{
    public class FriendsEntryUIBlocked : FriendsEntryUI
    {
        [SerializeField] private Button unblockButton;

        private void OnEnable() => unblockButton.onClick.AddListener(Unblock);
        private void OnDisable() => unblockButton.onClick.RemoveListener(Unblock);
        private void Unblock() => FriendsManager.Unblock(ID);
    }
}