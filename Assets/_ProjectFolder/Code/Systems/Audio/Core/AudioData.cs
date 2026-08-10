namespace UnityEngine.Audio
{
    public enum Channel
    {
        Music,
        SoundFx,
        WorldSpace
    }
    public class AudioClipCache
    {
        public IAudioGenerator audio;
        public byte refCount;
    }
}