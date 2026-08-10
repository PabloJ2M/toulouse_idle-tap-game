using System;

namespace UnityEngine.Audio
{
    [CreateAssetMenu(fileName = "settings", menuName = "system/audio/settings")]
    public class AudioSettings : ScriptableObject
    {
        [SerializeField] private Channel _channel;
        [SerializeField, Range(0f, 1f)] private float _defaultVolume = 0.5f;

        private string _volumeID => $"Volume_{_channel}";
        [SerializeField] private float _currentVolume;
        public Action<float> onVolumeChanged;

        private string _muteID => $"Mute_{_channel}";
        [SerializeField] private bool _isMuted;
        public Action<bool> onMuteChanged;

        public void LoadVolumeData()
        {
            _currentVolume = PlayerPrefs.GetFloat(_volumeID, _defaultVolume);
            RefreshVolume();
        }
        public void SetVolume(float value)
        {
            PlayerPrefs.SetFloat(_volumeID, value);
            _currentVolume = value;
            RefreshVolume();
        }
        public float GetVolume() => _currentVolume;
        public void RefreshVolume() => onVolumeChanged?.Invoke(_currentVolume);

        public void LoadMuteData()
        {
            _isMuted = PlayerPrefs.GetInt(_muteID) == 1;
            RefreshMute();
        }
        public void SetMute(bool value)
        {
            PlayerPrefs.SetInt(_muteID, value ? 1 : 0);
            _isMuted = value;
            RefreshMute();
        }
        public bool IsMuted() => _isMuted;
        public void RefreshMute() => onMuteChanged?.Invoke(_isMuted);
    }
}