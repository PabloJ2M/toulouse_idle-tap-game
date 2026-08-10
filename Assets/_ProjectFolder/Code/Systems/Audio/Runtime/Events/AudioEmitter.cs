namespace UnityEngine.Audio
{
    using AddressableAssets;

    public class AudioEmitter : AudioEmitterBehaviour
    {
        [Header("Audio Reference")]
        [SerializeField] protected AssetReferenceT<AudioClip> _audioReference;
        [SerializeField] private bool _preloadAsset, _playOnAwake;

        protected bool _hasLoaded;

        private async void Start()
        {
            if (_preloadAsset)
            {
                await _manager.LoadAudioAsset(_audioReference);
                _hasLoaded = true;
            }

            if (_playOnAwake) Play();
        }
        private void OnDestroy()
        {
            if (!_hasLoaded) return;
            _manager.UnloadAudioAsset(_audioReference);
        }

        public override async void Play()
        {
            var audio = await _manager.LoadAudioAsset(_audioReference, _hasLoaded);
            _hasLoaded = true;

            _manager.Play(_channel, audio);
        }
        public override async void PlayOneShot()
        {
            var audio = await _manager.LoadAudioAsset(_audioReference, _hasLoaded);
            _hasLoaded = true;

            _manager.PlayOneShot(_channel, audio);
        }
    }
}