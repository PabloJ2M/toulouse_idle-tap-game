namespace UnityEngine.Pool
{
    public interface IObjectPooled
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
        
        IObjectPool<IObjectPooled> Reference { get; set; }

        void SetPoolReference(IObjectPool<IObjectPooled> pool) => Reference = pool;
        
        void Enable();
        void Disable();
        
        void Release();
        void Destroy();
    }
}