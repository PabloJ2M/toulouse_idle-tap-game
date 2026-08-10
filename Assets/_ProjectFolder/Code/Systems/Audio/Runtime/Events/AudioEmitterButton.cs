namespace UnityEngine.Audio
{
    using UI;

    [RequireComponent(typeof(Button))]
    public class AudioEmitterButton : AudioEmitterAction
    {
        protected override void Awake()
        {
            base.Awake();
            Button button = GetComponent<Button>();

            switch (_channel) {
                case Channel.Music: button.onClick.AddListener(Play); break;
                case Channel.SoundFx: button.onClick.AddListener(PlayOneShot); break;
            }
        }
    }
}