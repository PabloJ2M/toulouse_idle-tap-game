namespace UnityEngine.Animations
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TweenCanvasGroup : TweenTransparency
    {
        [SerializeField] private bool modifyInteraction;
        private CanvasGroup _canvasGroup;

        protected override void Awake()
        {
            base.Awake();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void OnPlay(bool value)
        {
            base.OnPlay(value);
            PerformInteraction(value);
        }
        protected override void OnUpdate(float value)
        {
            base.OnUpdate(value);
            _canvasGroup.alpha = value;
        }

        private void PerformInteraction(bool value)
        {
            if (modifyInteraction)
                _canvasGroup.interactable = _canvasGroup.blocksRaycasts = value;
        }
    }
}