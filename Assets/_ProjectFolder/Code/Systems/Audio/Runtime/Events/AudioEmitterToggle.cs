namespace UnityEngine.Audio
{
    using UI;

    [RequireComponent(typeof(Toggle))]
    public class AudioEmitterToggle : AudioEmitterAction
    {
        protected override void Awake()
        {
            base.Awake();
            Toggle button = GetComponent<Toggle>();

            switch (_channel) {
                case Channel.Music: button.onValueChanged.AddListener(PlayHandler); break;
                case Channel.SoundFx: button.onValueChanged.AddListener(PlayOneShotHandler); break;
            }
        }

        private void PlayHandler(bool value)
        {
            if (value)
                Play();
        }
        private void PlayOneShotHandler(bool value)
        {
            if (value)
                PlayOneShot();
        }
    }
}