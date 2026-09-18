using System.Collections.Generic;

namespace UnityEngine.Pool
{
    [CreateAssetMenu(fileName = "PoolList", menuName = "system/pool/pooling probability")]
    public class ScriptablePoolListProbability : ScriptablePoolListDictionary<PoolObject, int>
    {
        public override IObjectPooled this[int index] => keys[index];
        public override IReadOnlyCollection<IObjectPooled> Prefabs => keys;

        public override IObjectPooled RandomPrefab => this[Random.Range(0, Count)];
        public override bool Contains(IObjectPooled prefab) => Dictionary.ContainsKey((PoolObject)prefab);
        
        public IObjectPooled RandomByProbability()
        {
            if (keys == null || keys.Count == 0)
                return null;
 
            for (var i = 0; i < keys.Count; i++) {
                if (values[i] != 0 && Random.Range(0f, 100f) <= values[i])
                    return this[i];
            }
            
            return this[^1];
        }
    }
}