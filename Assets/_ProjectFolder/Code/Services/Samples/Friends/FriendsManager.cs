using System;
using UnityEngine;

namespace Unity.Services.Friends.Samples
{
    public class FriendsManager : FriendsModule
    {
        public static event Action OnRefresh;
        
        public void Refresh() => _ = RefreshRelationships();
        private static async Awaitable RefreshRelationships()
        {
            await Service.Refresh();
            OnRefresh?.Invoke();
        }
        
        public static void AddFriend(string memberId) => _ = Service.SendFriendRequest(memberId);
        public static void AddFriendByName(string name) => _ = Service.SendFriendRequestByName(name);
        
        public static void CancelRequest(string memberId) => _ = Service.CancelFriendRequest(memberId);
        
        public static void DeclineFriend(string memberId) => _ = Service.CancelIncomingRequest(memberId);
        public static void RemoveFriend(string memberId) => _ = Service.RemoveRelationship(memberId);
        
        public static void Block(string memberId) => _ = Service.AddBlock(memberId);
        public static void Unblock(string memberId) => _ = Service.RemoveBlock(memberId);
    }
}