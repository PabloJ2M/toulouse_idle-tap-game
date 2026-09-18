namespace UnityEngine.Pool
{
    public class PoolObject : MonoBehaviour, IObjectPooled
    {
        [field: SerializeField] public GameObject GameObject { get; private set; }
        [field: SerializeField] public Transform Transform { get; private set; }
        
        public IObjectPool<IObjectPooled> Reference { get; set; }
        public EntityId EntityId => gameObject.GetEntityId();

        protected virtual void Reset()
        {
            GameObject = gameObject;
            Transform = transform;
        }
        
        public virtual void Enable() => GameObject.SetActive(true);
        public virtual void Disable() => GameObject.SetActive(false);
        public virtual void Destroy() => Destroy(GameObject);

        public virtual void Release()
        {
            if (GameObject.activeSelf)
                Reference.Release(this);
        }
    }
}