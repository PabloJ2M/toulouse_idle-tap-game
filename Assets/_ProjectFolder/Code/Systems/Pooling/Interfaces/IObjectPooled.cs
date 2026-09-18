namespace UnityEngine.Pool
{
    public interface IObjectPooled
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
        EntityId EntityId { get; }
        
        IObjectPool<IObjectPooled> Reference { get; set; }
        
        void Enable();
        void Disable();
        
        void Release();
        void Destroy();
    }
}