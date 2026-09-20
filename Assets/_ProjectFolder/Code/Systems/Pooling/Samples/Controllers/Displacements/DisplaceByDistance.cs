namespace UnityEngine.Pool
{
    public class DisplaceByDistance : DisplaceBehaviour<SpawnerByDistance>
    {
        [SerializeField] private float multiplier = 1f;
        [SerializeField] private Vector3 axis = Vector3.up;

        // private void FixedUpdate() => Translate(Time.fixedDeltaTime);

        public override void Translate(float value)
        {
            var amount = value * multiplier;
            var direction = amount * axis;
            
            for (var i = Manager.LastIndex; i >= 0; i--)
                Manager.Spawned[i].Transform.Translate(direction);
            
            Manager.OnTranslate(amount);
        }
    }
}