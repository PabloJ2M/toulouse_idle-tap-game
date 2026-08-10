namespace UnityEngine.Audio
{
    public interface ISourceChannel
    {
        Channel Channel { get; }
        AudioSource Source { get; }
        AudioMixerGroup MixerGroup { get; }

        void Play(IAudioGenerator audio);
        void PlayAtPoint(IAudioGenerator audio, Vector3 position);
        void PlayOneShot(IAudioGenerator audio, float pitch = 1f);
        void PlayOneShotAtPoint(IAudioGenerator audio, Vector3 position, float pitch = 1f);
        void ResetAudio();
    }
}