using System.Collections.Generic;

namespace UnityEngine.Pool
{
    [CreateAssetMenu(fileName = "PoolList", menuName = "system/pool/pooling list")]
    public class ScriptablePoolListHash : ScriptablePoolListBase, ISerializationCallbackReceiver
    {
        [SerializeField] private PoolObject[] prefabs;
        private readonly HashSet<PoolObject> _hash = new();
        
        public override IReadOnlyCollection<IObjectPooled> Prefabs => _hash;
        
        public override IObjectPooled this[int index] => prefabs[index];
        public override IObjectPooled RandomPrefab => this[Random.Range(0, prefabs.Length)];
        
        public override bool Contains(IObjectPooled prefab) => _hash.Contains((PoolObject)prefab);

        #region HashSet Validation
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            _hash.Clear();
            if (prefabs == null) return;

            foreach (var @object in prefabs)
            {
                if (@object)
                    _hash.Add(@object);
            }
        }
        private void OnValidate()
        {
            if (prefabs.Length == 0) return;
            
            HashSet<PoolObject> uniqueItems = new();
            List<PoolObject> cleanedList = new();
            bool hasDuplicates = false;

            foreach (var prefab in prefabs)
            {
                if (uniqueItems.Add(prefab)) {
                    cleanedList.Add(prefab);
                    continue;
                }
                
                cleanedList.Add(null);
                hasDuplicates = true;
            }
            
            if (hasDuplicates)
                prefabs = cleanedList.ToArray();
        }
        #endregion
    }
}