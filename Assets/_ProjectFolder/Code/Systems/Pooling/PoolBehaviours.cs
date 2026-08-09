using System.Collections.Generic;

namespace UnityEngine.Pool
{
    public abstract class PoolBehaviour : PoolBase
    {
        [SerializeField] protected PoolSettings poolSettings;
        
        private IObjectPool<IObjectPooled> _pool;

        protected virtual void Awake() => _pool = poolSettings.Create(OnCreate, new(OnGet, OnRelease, OnDestroyObject));
        protected virtual IObjectPooled OnCreate()
        {
            var @object = Instantiate(poolSettings.Prefab, poolSettings.Container).GetComponent<IObjectPooled>();
            @object.SetPoolReference(_pool);
            return @object;
        }

        protected IObjectPooled GetPrefab() => _pool.Get();
    }
    public abstract class PoolArrayBehaviour : PoolBase
    {
        [SerializeField] protected PoolArraySettings poolSettings;

        private IDictionary<EntityId, IObjectPool<IObjectPooled>> _pools;
        
        protected virtual void Awake() => poolSettings.Create(ref _pools, OnCreate, new(OnGet, OnRelease, OnDestroyObject));
        protected virtual IObjectPooled OnCreate(PoolObject prefab)
        {
            var obj = Instantiate(prefab, poolSettings.Container).GetComponent<IObjectPooled>();
            obj.SetPoolReference(_pools[prefab.GetEntityId()]);
            return obj;
        }

        protected IObjectPooled GetPrefabByEntityId(EntityId entityId) => _pools[entityId].Get();
        protected IObjectPooled GetPrefabByIndex(int index) => GetPrefabByEntityId(poolSettings.List[index].GetEntityId());
        protected IObjectPooled GetPrefabRandom() => GetPrefabByEntityId(poolSettings.List.RandomPrefab.GetEntityId());
    }
}