namespace UnityEngine.Animations
{
    public abstract class TweenCustomVector : TweenCustom
    {
        [SerializeField] protected Axis axis = Axis.X | Axis.Y | Axis.Z;
        [SerializeField] protected AnimationCurve animationCurve;

        protected Transform Transform;
        protected Vector3 From, To;

        protected override void Awake()
        {
            base.Awake();
            Transform = transform;
            if (animationCurve.length == 0)
                Debug.LogWarning("Animation Curve is empty");
        }
        protected override void OnStart()
        {
            base.OnStart();
            if (animationCurve.length != 0)
                OnUpdate(Current);
        }
        protected override void OnPlay(bool value)
        {
            if (animationCurve.length != 0)
                base.OnPlay(value);
        }
    }
}