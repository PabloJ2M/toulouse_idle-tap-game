using System.Collections.Generic;

namespace UnityEngine.Pool
{
    public abstract class ScriptablePoolListBase : ScriptableObject
    {
        public abstract PoolObject this[int index] { get; }
        public abstract IReadOnlyCollection<PoolObject> Prefabs { get; }
        public int Count => Prefabs.Count;

        public abstract PoolObject RandomPrefab { get; }
        public abstract bool Contains(PoolObject prefab);
    }
}