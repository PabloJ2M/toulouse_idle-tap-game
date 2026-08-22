using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Friends.Samples.UI
{
    public class FriendsEntryUIRequest : FriendsEntryUI
    {
        [SerializeField] private Button acceptButton, declineButton, blockButton;

        private void OnEnable()
        {
            acceptButton.onClick.AddListener(AcceptFriendRequest);
            declineButton.onClick.AddListener(DeclineFriendRequest);
            blockButton.onClick.AddListener(Block);
        }
        private void OnDisable()
        {
            acceptButton.onClick.RemoveListener(AcceptFriendRequest);
            declineButton.onClick.RemoveListener(DeclineFriendRequest);
            blockButton.onClick.RemoveListener(Block);
        }

        private void AcceptFriendRequest() => FriendsManager.AddFriend(ID);
        private void DeclineFriendRequest() => FriendsManager.DeclineFriend(ID);
        private void Block() => FriendsManager.Block(ID);
    }
}