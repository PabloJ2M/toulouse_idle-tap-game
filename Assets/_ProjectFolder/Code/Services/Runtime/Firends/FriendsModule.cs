using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Friends
{
    using Core;
    using Models;
    
    public abstract class FriendsModule : ServicesStatic<FriendsModule>, IServiceAuthModule
    {
        protected static IFriendsService Service { get; private set; }
        
        public IReadOnlyList<Relationship> Friends => Service?.Friends;
        public IReadOnlyList<Relationship> IncomingRequests => Service?.IncomingFriendRequests;
        public IReadOnlyList<Relationship> OutgoingRequests => Service?.OutgoingFriendRequests;
        public IReadOnlyList<Relationship> Blocked => Service?.Blocks;

        public virtual void OnUserSignedIn(string playerID) => _ = Initialize();
        public virtual void OnUserSignedOut() { }

        private static async Awaitable Initialize()
        {
            Service = FriendsService.Instance;
            await Service.InitializeAsync().Response();
        }
    }
}