using System.Collections;

namespace UnityEngine.Pool.Effects
{
    using Events;
    
    public class ParticleBase : PoolObject
    {
        [SerializeField, Min(0)] private float lifetime;
        [SerializeField] private UnityEvent<float> onParticleUpdate;
        
        public override void Enable()
        {
            base.Enable();
            StartCoroutine(LifeTime());
        }
        public override void Disable()
        {
            base.Disable();
            StopAllCoroutines();
        }
        
        private IEnumerator LifeTime()
        {
            float time = 0f;

            while (time < lifetime)
            {
                onParticleUpdate.Invoke(1f - time / lifetime);
                time += Time.deltaTime;
                yield return null;
            }
            
            Release();
        }
    }
}