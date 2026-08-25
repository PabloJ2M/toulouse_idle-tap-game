namespace UnityEngine.Pool
{
    public abstract class SpawnerByDistance : PoolArrayBehaviour
    {
        [SerializeField] private float distance;
        private float _travelled;
        
        public void OnTranslate(float value)
        {
            _travelled += value;
            
            if (_travelled < distance) return;
            _travelled %= distance;
            OnSpawn();
        }
        
        protected abstract void OnSpawn();
    }
}