namespace UnityEngine.Pool
{
    public abstract class ParticlesBase : PoolBehaviour
    {
        protected abstract void DropParticle(IObjectPooled instance);

        protected void SetImage(IObjectPooled particle, IObjectPooled copy)
        {
            particle.Transform.GetComponent<SpriteRenderer>().sprite =
                copy.Transform.GetComponent<SpriteRenderer>().sprite;
        }
    }
    
    public abstract class ParticlesBehaviour : ParticlesBase
    {
        private PoolBehaviourEvents _behaviourEvents;

        protected override void Awake()
        {
            base.Awake();
            _behaviourEvents = GetComponent<PoolBehaviourEvents>();
        }

        protected void OnEnable() => _behaviourEvents.OnDespawn += DropParticle;
        protected void OnDisable() => _behaviourEvents.OnDespawn -= DropParticle;
    }
    public abstract class ParticlesArrayBehaviour : ParticlesBase
    {
        private PoolArrayBehaviourEvents _behaviourEvents;

        protected override void Awake()
        {
            base.Awake();
            _behaviourEvents = GetComponent<PoolArrayBehaviourEvents>();
        }

        protected void OnEnable() => _behaviourEvents.OnDespawn += DropParticle;
        protected void OnDisable() => _behaviourEvents.OnDespawn -= DropParticle;
    }
}
