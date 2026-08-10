using System.Collections.Generic;

namespace UnityEngine.Audio
{
    using AddressableAssets;

    public class AudioManager : Singleton<AudioManager>, IAudioManager
    {
        private readonly Dictionary<AssetReference, AudioClipCache> _audioClipCache = new();
        private readonly Dictionary<Channel, ISourceChannel> _channelMap = new();

        protected override void Awake()
        {
            base.Awake();
            var channels = GetComponentsInChildren<ISourceChannel>();

            foreach (var channel in channels)
                _channelMap.Add(channel.Channel, channel);
        }

        public async Awaitable<IAudioGenerator> LoadAudioAsset(AssetReferenceT<AudioClip> reference, bool hasLoaded = false)
        {
            if (_audioClipCache.TryGetValue(reference, out var cache)) {
                if (!hasLoaded) cache.refCount++;
                return cache.audio;
            }

            var clip = await reference.LoadAssetAsync().Task;
            _audioClipCache.Add(reference, new() { audio = clip, refCount = 1 });
            return clip;
        }
        public void UnloadAudioAsset(AssetReferenceT<AudioClip> reference)
        {
            if (!_audioClipCache.TryGetValue(reference, out var cache)) return;
            cache.refCount--;

            if (cache.refCount > 0) return;

            reference.ReleaseAsset();
            _audioClipCache.Remove(reference);
        }

        public void Play(Channel channel, IAudioGenerator audio)
        {
            if (_channelMap.TryGetValue(channel, out var source))
                source.Play(audio);
        }
        public void PlayAtPoint(Channel channel, IAudioGenerator audio, Vector3 position)
        {
            if (_channelMap.TryGetValue(channel, out var source))
                source.PlayAtPoint(audio, position);
        }
        public void PlayOneShot(Channel channel, IAudioGenerator audio, float pitch = 1f)
        {
            if (_channelMap.TryGetValue(channel, out var source))
                source.PlayOneShot(audio, pitch);
        }
        public void PlayOneShotAtPoint(Channel channel, IAudioGenerator audio, Vector3 position, float pitch = 1f)
        {
            if (_channelMap.TryGetValue(channel, out var source))
                source.PlayOneShotAtPoint(audio, position, pitch);
        }

        public void ResetAudio(Channel channel)
        {
            if (_channelMap.TryGetValue(channel, out var source))
                source.ResetAudio();
        }
    }
}