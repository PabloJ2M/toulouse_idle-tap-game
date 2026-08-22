using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Friends
{
    using Models;
    using Exceptions;
    
    public static class FriendsExtensions
    {
        public static async Awaitable Response(this Task action)
        {
            try { await action; }
            catch (Exception e) { Debug.LogError(e); }
        }
        private static async Awaitable<T> Response<T>(this Task<T> action) where T : class
        {
            try { return await action; }
            catch (FriendsServiceException e) { Debug.LogError(e); }
            return null;
        }
        public static async Awaitable<T> Response<T>(this Awaitable<T> action) where T : class
        {
            try { return await action; }
            catch (FriendsServiceException e) { Debug.LogError(e); }
            return null;
        }

        public static async Awaitable Refresh(this IFriendsService service) =>
            await service.ForceRelationshipsRefreshAsync().Response();
        
        public static async Awaitable<Relationship> SendFriendRequest(this IFriendsService service, string memberId) =>
            await service.AddFriendAsync(memberId).Response();
        
        public static async Awaitable<Relationship> SendFriendRequestByName(this IFriendsService service, string name) =>
            await service.AddFriendByNameAsync(name).Response();
        
        public static async Awaitable CancelFriendRequest(this IFriendsService service, string memberId) =>
            await service.DeleteOutgoingFriendRequestAsync(memberId).Response();
        
        public static async Awaitable CancelIncomingRequest(this IFriendsService service, string memberId) =>
            await service.DeleteIncomingFriendRequestAsync(memberId).Response();
        
        public static async Awaitable RemoveRelationship(this IFriendsService service, string memberId) =>
            await service.DeleteRelationshipAsync(memberId).Response();
        
        public static async Awaitable DeleteFriend(this IFriendsService service, string memberId) =>
            await service.DeleteFriendAsync(memberId).Response();
        
        public static async Awaitable<Relationship> AddBlock(this IFriendsService service, string memberId) =>
            await service.AddBlockAsync(memberId).Response();
        
        public static async Awaitable RemoveBlock(this IFriendsService service, string memberId) =>
            await service.DeleteBlockAsync(memberId).Response();
    }
}