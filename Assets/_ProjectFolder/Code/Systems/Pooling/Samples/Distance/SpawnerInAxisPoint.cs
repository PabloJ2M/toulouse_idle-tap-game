namespace UnityEngine.Pool
{
    public class SpawnerInAxisPoint : SpawnerByDistance
    {
        protected override void OnSpawn()
        {
            IObjectPooled objectPooled = GetPrefabRandom();
            objectPooled.Transform.localPosition = Vector3.zero;
        }
    }
}
