namespace UnityEngine.Pool
{
    [RequireComponent(typeof(PoolBase))]
    public abstract class DisplaceBehaviour<T> : MonoBehaviour where T : PoolBase
    {
        protected T Manager;

        protected virtual void Awake() => Manager = GetComponent<T>();

        public abstract void Translate(float value);

        public void TranslateUnit() => Translate(1f);
        public void TranslateUnitBackwards() => Translate(-1f);
    }
    
    public abstract class DisplaceBehaviour : DisplaceBehaviour<PoolBase>
    { }
}
