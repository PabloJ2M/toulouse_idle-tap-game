namespace UnityEngine.Animations
{
    using UI;

    [RequireComponent(typeof(Image))]
    public class TweenImageColor : TweenColor
    {
        private Image _image;

        protected override void Awake()
        {
            base.Awake();
            _image = GetComponent<Image>();
            Default = _image.color;
        }
        protected override void OnUpdate(float value)
        {
            base.OnUpdate(value);
            var time = animationCurve.Evaluate(value);
            _image.color = Color.Lerp(Default, Target, time);
        }
    }
}