namespace UnityEngine.Audio
{
    [RequireComponent(typeof(ISourceChannel))]
    public class AudioSettingsListener : MonoBehaviour
    {
        [SerializeField] private AudioSettings _settings;
        [SerializeField] private string _volumeMixerParam;

        private ISourceChannel _channel;

        private void Awake() => _channel = GetComponent<ISourceChannel>();

        private void Start()
        {
            _settings.LoadVolumeData();
            _settings.LoadMuteData();
        }
        private void OnEnable()
        {
            _settings.onVolumeChanged += OnVolumeChanged;
            _settings.onMuteChanged += OnMuteChanged;
        }
        private void OnDisable()
        {
            _settings.onVolumeChanged -= OnVolumeChanged;
            _settings.onMuteChanged -= OnMuteChanged;
        }

        private void OnMuteChanged(bool value)
        {
            if (_channel.Source)
                _channel.Source.mute = value;
        }
        private void OnVolumeChanged(float value)
        {
            float dB = Mathf.Log10(Mathf.Max(0.001f, value)) * 20f;
            _channel.MixerGroup.audioMixer.SetFloat(_volumeMixerParam, dB);
        }
    }
}