namespace UnityEngine.Audio
{
    public class SingleChannel : AudioChannel
    {
        private AudioSource _source;
        public override AudioSource Source => _source;

        private void Awake()
        {
            _source = GetComponentInChildren<AudioSource>();
            _source.outputAudioMixerGroup = _mixerGroup;
            _defaultResource = _source.generator;
        }
    }
}