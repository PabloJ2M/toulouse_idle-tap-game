namespace UnityEngine.Pool.Effects
{
    public class ParticlePhysics : ParticleBase
    {
        private Rigidbody2D _rigidbody;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public override void Enable()
        {
            base.Enable();
            _rigidbody.SetRotation(0);
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
        public override void Disable()
        {
            base.Disable();
            _rigidbody.bodyType = RigidbodyType2D.Static;
        }

        public void AddForce(Vector2 force) => _rigidbody.AddForce(force, ForceMode2D.Impulse);
        public void AddTorque(float torque) => _rigidbody.AddTorque(torque);
    }
}