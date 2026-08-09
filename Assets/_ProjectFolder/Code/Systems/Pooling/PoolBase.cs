using System.Collections.Generic;

namespace UnityEngine.Pool
{
    public abstract class PoolBase : MonoBehaviour
    {
        public IList<IObjectPooled> Spawned => _spawned;
        public int LastIndex => _spawned.Count - 1;
        
        private readonly List<IObjectPooled> _spawned = new();
        
        protected virtual void OnGet(IObjectPooled @object)
        {
            _spawned.Add(@object);
            @object.Enable();
        }
        protected virtual void OnRelease(IObjectPooled @object)
        {
            _spawned.Remove(@object);
            @object.Disable();
        }
        protected static void OnDestroyObject(IObjectPooled @object) => @object.Destroy();
        
        protected void ReleaseFirst()
        {
            if (_spawned.Count > 0)
                _spawned[0].Release();
        }
        protected void ReleaseLast()
        {
            if (_spawned.Count > 0)
                _spawned[LastIndex].Release();
        }
        protected void Clear()
        {
            for (int i = LastIndex; i >= 0; i--)
                _spawned[i].Release();
        }
    }
}