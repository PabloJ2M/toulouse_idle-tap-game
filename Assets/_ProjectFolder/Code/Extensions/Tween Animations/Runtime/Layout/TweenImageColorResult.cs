namespace UnityEngine.Animations
{
    public class TweenImageColorResult : TweenImageColor
    {
        [Header("Alternative Color")]
        [SerializeField] private AnimationCurve altAnimationCurve;
        [SerializeField] private Color altColor = Color.red;

        private AnimationCurve _defaultAnimationCurve;

        protected override void Awake()
        {
            base.Awake();
            _defaultAnimationCurve = altAnimationCurve;
        }
        protected override void OnPlay(bool value)
        {
            Current = 0;
            Target = value ? color : altColor;
            animationCurve = value ? _defaultAnimationCurve : altAnimationCurve;

            base.OnPlay(true);
        }
    }
}