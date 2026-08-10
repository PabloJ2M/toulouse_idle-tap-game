using UnityEngine.AddressableAssets;

namespace UnityEngine.Audio
{
    public interface IAudioManager
    {
        Awaitable<IAudioGenerator> LoadAudioAsset(AssetReferenceT<AudioClip> reference, bool hasLoaded = false);
        void UnloadAudioAsset(AssetReferenceT<AudioClip> reference);

        void Play(Channel type, IAudioGenerator key);
        void PlayAtPoint(Channel channel, IAudioGenerator audio, Vector3 position);
        void PlayOneShot(Channel type, IAudioGenerator key, float pitch = 1f);
        void PlayOneShotAtPoint(Channel type, IAudioGenerator key, Vector3 position, float pitch = 1f);
        void ResetAudio(Channel type);
    }
}