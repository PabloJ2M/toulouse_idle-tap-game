namespace UnityEngine.Animations
{
    public abstract class TweenColor : TweenCustom
    {
        [SerializeField] protected AnimationCurve animationCurve;
        [SerializeField] protected Color color = Color.white;

        protected Color Default = Color.white, Target = Color.white;

        protected override void Awake()
        {
            base.Awake();
            Target = color;
            if (animationCurve.length == 0)
                Debug.LogWarning("Animation Curve is empty");
        }
        protected override void OnStart()
        {
            base.OnStart();
            OnUpdate(Current);
        }

        [ContextMenu("ColorIn")] private void ColorIn() => TweenCore.Play(true);
        [ContextMenu("ColorOut")] private void ColorOut() => TweenCore.Play(false);
        
        [ContextMenu("ForceColorIn")] private void ForceColorIn() => TweenCore.ForcePlay(true);
        [ContextMenu("ForceColorOut")] private void ForceColorOut() => TweenCore.ForcePlay(false);
    }
}