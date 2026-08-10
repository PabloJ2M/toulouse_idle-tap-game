namespace UnityEngine.Audio
{
    public abstract class AudioEmitterBehaviour : MonoBehaviour
    {
        [SerializeField] protected Channel _channel = Channel.SoundFx;

        protected IAudioManager _manager;

        protected virtual void Awake() => _manager = AudioManager.Instance;

        public abstract void Play();
        public abstract void PlayOneShot();
        public void ResetAudio() => _manager?.ResetAudio(_channel);
    }
}