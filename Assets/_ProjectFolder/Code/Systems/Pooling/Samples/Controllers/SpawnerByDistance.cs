namespace UnityEngine.Pool
{
    public abstract class SpawnerByDistance : PoolArrayBehaviourEvents
    {
        [SerializeField] private float distance;
        
        protected float Travelled { get; private set; }

        public void OnTranslate(float value)
        {
            Travelled += value;
            
            if (Travelled < distance) return;
            Travelled %= distance;
            OnSpawnItem();
        }
        
        protected abstract void OnSpawnItem();
    }
}