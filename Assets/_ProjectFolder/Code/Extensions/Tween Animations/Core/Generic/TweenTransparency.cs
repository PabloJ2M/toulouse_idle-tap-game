namespace UnityEngine.Animations
{
    [DefaultExecutionOrder(-25)]
    public abstract class TweenTransparency : TweenCustom
    {
        [SerializeField] private bool disableOnHidden;

        protected override void OnStart()
        {
            base.OnStart();
            OnUpdate(Current);
        }
        protected override void OnComplete()
        {
            base.OnComplete();

            if (Mathf.Approximately(Current, 0f))
                PerformVisibility(false);
        }
        protected override void OnPlay(bool value)
        {
            PerformVisibility(true);
            base.OnPlay(value);
        }

        private void PerformVisibility(bool value)
        {
            if (disableOnHidden)
                gameObject.SetActive(value);
        }

        [ContextMenu("FadeIn")]
        public void FadeIn()
        {
            PerformVisibility(true);
            TweenCore?.Play(true);
        }

        [ContextMenu("FadeOut")]
        public void FadeOut()
        {
            TweenCore?.Play(false);
        }
    }
}