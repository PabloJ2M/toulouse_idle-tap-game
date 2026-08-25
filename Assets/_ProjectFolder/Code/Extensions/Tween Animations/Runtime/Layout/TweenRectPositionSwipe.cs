namespace UnityEngine.Animations
{
    public class TweenRectPositionSwipe : TweenRectPosition
    {
        protected override void OnStart() => RectTransform.localPosition = !TweenCore.IsEnabled ? To : From;

        [ContextMenu("SwipeIn")] public void SwipeIn() => TweenCore?.Play(true);
        [ContextMenu("SwipeOut")] public void SwipeOut() => TweenCore?.Play(false);
    }
}