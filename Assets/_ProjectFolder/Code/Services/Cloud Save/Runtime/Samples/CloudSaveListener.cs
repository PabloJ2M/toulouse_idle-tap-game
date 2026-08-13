using UnityEngine;

namespace Unity.Services.CloudSave
{
    using Models;
    
    public abstract class CloudSaveListener : MonoBehaviour
    {
        [SerializeField] protected string dataID;
        
        private CloudSaveManager _saveManager;

        protected virtual void Awake() => _saveManager = CloudSaveManager.Instance;
        protected virtual void OnEnable()
        {
            _saveManager.OnCloudSaveFetch += OnCloudSaveFetch;
            _saveManager.OnCloudSaveClear += OnCloudSaveClear;
        }
        protected virtual void OnDisable()
        {
            _saveManager.OnCloudSaveFetch -= OnCloudSaveFetch;
            _saveManager.OnCloudSaveClear -= OnCloudSaveClear;
        }
        private void OnCloudSaveFetch(string key, Item item)
        {
            if (key == dataID)
                OnCloudSaveUpdate(item);
        }

        protected abstract void OnCloudSaveUpdate(Item item);
        protected abstract void OnCloudSaveClear();
        
        public abstract void Save();
    }
}