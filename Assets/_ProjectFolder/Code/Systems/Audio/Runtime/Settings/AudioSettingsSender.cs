namespace UnityEngine.Audio
{
    public abstract class AudioSettingsSender : MonoBehaviour
    {
        [SerializeField] protected AudioSettings _settings;

        protected abstract void OnEnable();
        protected abstract void OnDisable();
    }
}