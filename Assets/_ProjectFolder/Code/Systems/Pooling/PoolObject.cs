namespace UnityEngine.Pool
{
    public class PoolObject : MonoBehaviour, IObjectPooled
    {
        public IObjectPool<IObjectPooled> Reference { get; set; }
        public GameObject GameObject { get; private set; }
        public Transform Transform { get; private set; }

        protected virtual void Awake()
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