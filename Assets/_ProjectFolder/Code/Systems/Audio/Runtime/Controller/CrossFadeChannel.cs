using System.Collections;

namespace UnityEngine.Audio
{
    public class CrossFadeChannel : AudioChannel
    {
        [SerializeField] private float _crossFadeTime = 1f;

        private AudioSource _active, _inactive;
        private float _time;

        public override AudioSource Source => _active;

        private void Awake()
        {
            var sources = GetComponentsInChildren<AudioSource>();
            _active = sources[0];
            _inactive = sources[1];

            _inactive.outputAudioMixerGroup = _active.outputAudioMixerGroup = _mixerGroup;
        }
        public override void Play(IAudioGenerator audio)
        {
            if (Source.generator == audio) return;

            StopAllCoroutines();
            StartCoroutine(Fade(audio));
        }

        private IEnumerator Fade(IAudioGenerator audio)
        {
            var inactive = _inactive;
            var active = _active;

            if (!inactive.isPlaying)
            {
                inactive.generator = audio;
                inactive.volume = 0;
                inactive.Play();
            }

            float invFade = 1f / _crossFadeTime;

            while (_time < _crossFadeTime)
            {
                _time += Time.deltaTime;
                float k = _time * invFade;

                active.volume = 1f - k;
                inactive.volume = k;

                yield return null;
            }

            active.volume = _time = 0f;
            inactive.volume = 1f;

            active.Stop();
            active.generator = null;

            _active = inactive;
            _inactive = active;
        }
    }
}