namespace UnityEngine.Pool
{
    public class DisplaceByDistance : DisplaceBehaviour<SpawnerByDistance>
    {
        [SerializeField] private float multiplier = 1f;
        [SerializeField] private Vector3 axis = Vector3.up;
        
        public override void Translate(float value)
        {
            var amount = value * multiplier * Time.fixedDeltaTime;
            var direction = amount * axis;
            
            for (var i = Manager.LastIndex; i >= 0; i--)
                Manager.Spawned[i].Transform.Translate(direction);
            
            Manager.OnTranslate(amount);
        }
    }
}