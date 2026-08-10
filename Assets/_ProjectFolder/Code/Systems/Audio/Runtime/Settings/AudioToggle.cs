namespace UnityEngine.Audio
{
    using UI;

    [RequireComponent(typeof(Toggle))]
    public class AudioToggle : AudioSettingsSender
    {
        private Toggle _toggle;

        protected void Awake() => _toggle = GetComponent<Toggle>();
        protected void Start() => _toggle.SetIsOnWithoutNotify(_settings.IsMuted());

        protected override void OnEnable() => _toggle.onValueChanged.AddListener(_settings.SetMute);
        protected override void OnDisable() => _toggle.onValueChanged.RemoveListener(_settings.SetMute);
    }
}