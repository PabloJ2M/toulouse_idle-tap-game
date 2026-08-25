namespace UnityEngine.Pool.Effects
{
    public class ParticlesPush : ParticlesArrayBehaviour
    {
        [Header("Effect Controller")]
        [SerializeField] private Vector2 direction;
        [SerializeField] private float torque;

        [SerializeField] private bool flipX;
        
        public void SetFlipX(bool value) => flipX = value;
        
        protected override void DropParticle(IObjectPooled instance)
        {
            var particle = GetPrefab() as ParticlePhysics;
            if (particle ==null) return;
            
            particle.Transform.position = instance.Transform.position;
            SetImage(particle, instance);
            
            var finalDirection = direction;
            finalDirection.x *= flipX ? -1 : 1;
            
            var finalTorque = torque;
            finalTorque *= flipX ? -1 : 1;
            
            particle?.AddForce(finalDirection);
            particle?.AddTorque(finalTorque);
        }
    }
}