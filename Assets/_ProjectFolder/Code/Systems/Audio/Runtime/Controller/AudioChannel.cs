namespace UnityEngine.Audio
{
    public abstract class AudioChannel : MonoBehaviour, ISourceChannel
    {
        [SerializeField] protected Channel _channel;
        [SerializeField] protected AudioMixerGroup _mixerGroup;

        protected IAudioGenerator _defaultResource;

        public Channel Channel => _channel;
        public AudioMixerGroup MixerGroup => _mixerGroup;
        public abstract AudioSource Source { get; }

        public virtual void Play(IAudioGenerator audio)
        {
            if (Source == null) return;
            if (Source.generator == audio) return;

            Source.generator = audio;
            Source.Play();
        }
        public virtual void PlayAtPoint(IAudioGenerator audio, Vector3 position) { }
        public virtual void PlayOneShot(IAudioGenerator audio, float pitch = 1f)
        {
            if (Source == null) return;
            if (audio is not AudioClip clip) return;

            Source.pitch = pitch;
            Source?.PlayOneShot(clip);
        }
        public virtual void PlayOneShotAtPoint(IAudioGenerator audio, Vector3 position, float pitch = 1f) { }

        public void ResetAudio() => Play(_defaultResource);
    }
}