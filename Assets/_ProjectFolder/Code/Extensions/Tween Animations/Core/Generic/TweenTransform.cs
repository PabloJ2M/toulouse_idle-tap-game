namespace UnityEngine.Animations
{
    public abstract class TweenTransform : TweenBehaviour<Vector3>
    {
        protected Transform Transform;
        protected Vector3 From, To;

        protected override void Awake()
        {
            base.Awake();
            Transform = transform;
        }
    }
}