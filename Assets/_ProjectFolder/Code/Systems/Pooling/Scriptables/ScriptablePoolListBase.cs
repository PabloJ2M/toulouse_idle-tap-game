using System.Collections.Generic;

namespace UnityEngine.Pool
{
    public abstract class ScriptablePoolListBase : ScriptableObject
    {
        public abstract IObjectPooled this[int index] { get; }
        public abstract IReadOnlyCollection<IObjectPooled> Prefabs { get; }
        public int Count => Prefabs.Count;

        public abstract IObjectPooled RandomPrefab { get; }
        public abstract bool Contains(IObjectPooled prefab);
    }
}