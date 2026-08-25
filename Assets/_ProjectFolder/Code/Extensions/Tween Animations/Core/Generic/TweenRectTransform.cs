namespace UnityEngine.Animations
{
    using UI;
    
    [RequireComponent(typeof(RectTransform))]
    public abstract class TweenRectTransform : TweenBehaviour<Vector3>
    {
        protected RectTransform RectTransform;
        protected Vector3 From, To;

        private bool _hasBuilt;

        protected override void Awake()
        {
            base.Awake();
            RectTransform = transform as RectTransform;
        }
        
        protected virtual void BuildUI() 
        {
            if (_hasBuilt) return;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
            _hasBuilt = true;
        }
    }
}