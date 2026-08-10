namespace UnityEngine.Audio
{
    using UI;

    [RequireComponent(typeof(Slider))]
    public class AudioSlider : AudioSettingsSender
    {
        private Slider _slider;

        protected void Awake() => _slider = GetComponent<Slider>();
        protected void Start() => _slider.SetValueWithoutNotify(_settings.GetVolume());

        protected override void OnEnable() => _slider.onValueChanged.AddListener(_settings.SetVolume);
        protected override void OnDisable() => _slider.onValueChanged.RemoveListener(_settings.SetVolume);
    }
}