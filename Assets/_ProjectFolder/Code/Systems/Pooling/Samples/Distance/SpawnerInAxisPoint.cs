namespace UnityEngine.Pool
{
    public class SpawnerInAxisPoint : SpawnerByDistance
    {
        protected override void OnSpawnItem()
        {
            IObjectPooled objectPooled = GetPrefabRandom();
            objectPooled.Transform.localPosition = Vector3.zero;
        }
    }
}
