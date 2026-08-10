namespace UnityEngine.Audio
{
    public abstract class AudioEmitterAction : AudioEmitter
    {
        [SerializeField] private Vector2 _pitch = Vector2.one;

        public override async void PlayOneShot()
        {
            var clip = await _manager.LoadAudioAsset(_audioReference, _hasLoaded);
            float pitch = _pitch.x == _pitch.y ? _pitch.x : Random.Range(_pitch.x, _pitch.y);

            _hasLoaded = true;

            _manager.PlayOneShot(_channel, clip, pitch);
        }
    }
}