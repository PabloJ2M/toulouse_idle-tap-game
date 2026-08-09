using System;

namespace UnityEngine.Pool
{
    public abstract class PoolBehaviourEvents : PoolBehaviour
    {
        public event Action<IObjectPooled> OnSpawn, OnDespawn;
        
        protected override void OnGet(IObjectPooled @object)
        {
            base.OnGet(@object);
            OnSpawn?.Invoke(@object);
        }
        protected override void OnRelease(IObjectPooled @object)
        {
            base.OnRelease(@object);
            OnDespawn?.Invoke(@object);
        }
    }
    public abstract class PoolArrayBehaviourEvents : PoolArrayBehaviour
    {
        public event Action<IObjectPooled> OnSpawn, OnDespawn;
        
        protected override void OnGet(IObjectPooled @object)
        {
            base.OnGet(@object);
            OnSpawn?.Invoke(@object);
        }
        protected override void OnRelease(IObjectPooled @object)
        {
            base.OnRelease(@object);
            OnDespawn?.Invoke(@object);
        }
    }
}