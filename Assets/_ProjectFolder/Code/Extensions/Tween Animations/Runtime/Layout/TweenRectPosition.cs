using PrimeTween;

namespace UnityEngine.Animations
{
    public abstract class TweenRectPosition : TweenRectTransform
    {
        [SerializeField] protected Direction direction;
        [SerializeField] protected Vector2 overrideDistance;

        protected override void Awake()
        {
            base.Awake();
            From = RectTransform.localPosition;
            BuildUI();
        }
        protected override void BuildUI()
        {
            base.BuildUI();
            var size = RectTransform.rect.size;
            var vector = direction.Get();

            if (overrideDistance.x != 0) size.x = overrideDistance.x;
            if (overrideDistance.y != 0) size.y = overrideDistance.y;

            To = From + new Vector3(vector.x * size.x, vector.y * size.y, 0f);
        }
        protected override void OnPlay(bool value)
        {
            base.OnPlay(value);

            TweenSettings = new(RectTransform.localPosition, value ? From : To, Settings);
            Tween = Tween.LocalPosition(RectTransform, TweenSettings);
            Tween.OnComplete(this, tween => tween.OnComplete());
        }
    }
}