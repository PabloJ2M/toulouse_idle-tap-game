using PrimeTween;

namespace UnityEngine.Animations
{
    public abstract class TweenCustom : TweenBehaviour<float>
    {
        protected float Current;

        protected override void OnStart() => Current = TweenCore.IsEnabled ? 1f : 0f;
        protected virtual void OnUpdate(float value) => Current = value;

        protected override void OnPlay(bool value)
        {
            base.OnPlay(value);

            TweenSettings = new(Current, value ? 1f : 0f, Settings);
            Tween = Tween.Custom(this, TweenSettings, (target, v) => target.OnUpdate(v));
            Tween.OnComplete(this, tween => tween.OnComplete());
        }
    }
}