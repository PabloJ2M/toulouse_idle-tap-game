using System.Collections.Generic;

namespace UnityEngine.Pool
{
    [CreateAssetMenu(fileName = "PoolList", menuName = "system/pool/pooling probability")]
    public class ScriptablePoolListProbability : ScriptablePoolListDictionary<PoolObject, int>
    {
        public override PoolObject this[int index] => keys[index];
        public override IReadOnlyCollection<PoolObject> Prefabs => keys;

        public override PoolObject RandomPrefab => GetRandomByProbability();
        public override bool Contains(PoolObject prefab) => Dictionary.ContainsKey(prefab);
        
        private PoolObject GetRandomByProbability()
        {
            if (keys == null || keys.Count == 0)
                return null;
 
            for (var i = 0; i < keys.Count; i++) {
                if (values[i] == 0) continue;
                if (Random.Range(0f, 100f) <= values[i])
                    return keys[i];
            }
            
            return keys[^1];
        }
    }
}