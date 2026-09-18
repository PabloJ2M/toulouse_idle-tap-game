using System.Collections.Generic;

namespace UnityEngine.Pool
{
    public abstract class PoolBehaviour : PoolBase
    {
        [SerializeField] protected PoolSettings poolSettings;
        
        private IObjectPool<IObjectPooled> _pool;

        protected virtual void Awake() => _pool = poolSettings.Init(OnCreate, new(OnGet, OnRelease, OnDestroyObject));
        protected virtual IObjectPooled OnCreate()
        {
            var @object = Instantiate(poolSettings.Prefab, poolSettings.Container).GetComponent<IObjectPooled>();
            @object.Reference = _pool;
            return @object;
        }

        protected IObjectPooled GetPrefab() => _pool.Get();
    }
    public abstract class PoolArrayBehaviour : PoolBase
    {
        [SerializeField] protected PoolArraySettings poolSettings;

        private IDictionary<EntityId, IObjectPool<IObjectPooled>> _pools;
        private readonly Queue<EntityId> _forcedEntities = new();
        
        protected virtual void Awake() => poolSettings.Init(ref _pools, OnCreate, new(OnGet, OnRelease, OnDestroyObject));
        protected virtual IObjectPooled OnCreate(IObjectPooled prefab)
        {
            var @object = Instantiate(prefab as PoolObject, poolSettings.Container);
            @object.Reference = _pools[prefab.EntityId];
            return @object;
        }

        public void ForceNext(IObjectPooled prefab) => _forcedEntities?.Enqueue(prefab.EntityId);
        
        private IObjectPooled GetPrefabByEntityId(EntityId entityId) =>
            _pools[entityId].Get();
        
        protected IObjectPooled GetPrefabByIndex(int index) =>
            GetPrefabByEntityId(poolSettings.List[index].EntityId);
        
        protected IObjectPooled GetPrefabRandom() =>
            _forcedEntities.Count == 0
            ? GetPrefabByEntityId(poolSettings.List.RandomPrefab.EntityId)
            : GetPrefabByEntityId(_forcedEntities.Dequeue());
    }
}