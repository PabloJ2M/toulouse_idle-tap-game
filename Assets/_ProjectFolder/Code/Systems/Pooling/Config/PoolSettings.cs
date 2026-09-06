using System;
using System.Collections.Generic;

namespace UnityEngine.Pool
{
    [Serializable]
    public abstract class PoolSettings<T> where T : Object
    {
        [SerializeField] private Transform container;
        [SerializeField] protected T reference;
        [Space]
        [SerializeField] private bool collectionCheck = true;
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxSize = 100;
        
        public Transform Container => container;
        
        public ObjectPool<IObjectPooled> Init(Func<IObjectPooled> create, PoolEvents events) =>
            new(create, events.OnGet, events.OnRelease, events.OnDestroy, collectionCheck, defaultCapacity, maxSize);
    }

    [Serializable]
    public class PoolSettings : PoolSettings<PoolObject>
    {
        public PoolObject Prefab => reference;
    }
    
    [Serializable]
    public class PoolArraySettings : PoolSettings<ScriptablePoolListBase>
    {
        public ScriptablePoolListBase List => reference;

        public void Init(ref IDictionary<EntityId, IObjectPool<IObjectPooled>> dictionary, Func<IObjectPooled, IObjectPooled> create, PoolEvents events)
        {
            dictionary = new Dictionary<EntityId, IObjectPool<IObjectPooled>>();
            
            foreach (var @object in List.Prefabs)
                dictionary.Add(@object.EntityId, Init(() => create?.Invoke(@object), events));
        }
    }
}