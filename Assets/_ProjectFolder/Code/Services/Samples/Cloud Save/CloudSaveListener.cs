using UnityEngine;

namespace Unity.Services.CloudSave
{
    using Models;
    
    public abstract class CloudSaveListener : MonoBehaviour
    {
        [SerializeField] protected string dataID;
        [SerializeField] protected SaveAccessType accessType;
        
        protected virtual void OnEnable()
        {
            CloudSaveManager.OnCloudSaveFetch += OnCloudSaveFetch;
            CloudSaveManager.OnCloudSaveClear += OnCloudSaveClear;
        }
        protected virtual void OnDisable()
        {
            CloudSaveManager.OnCloudSaveFetch -= OnCloudSaveFetch;
            CloudSaveManager.OnCloudSaveClear -= OnCloudSaveClear;
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