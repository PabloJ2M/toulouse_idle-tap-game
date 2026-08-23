namespace UnityEngine.Animations
{
    using Events;

    [RequireComponent(typeof(ITweenCallback))]
    public class TweenEvents : MonoBehaviour
    {
        [SerializeField] private UnityEvent<bool> onTweenStatusChanged, onTweenCompleted;
        [SerializeField] private UnityEvent onAnimationCompleted;

        private void Awake()
        {
            if (!TryGetComponent(out ITweenCallback tween)) return;
            tween.OnTweenStatusChanged += onTweenStatusChanged.Invoke;
            tween.OnTweenCompleted += onTweenCompleted.Invoke;
            tween.OnCompleted += onAnimationCompleted.Invoke;
        }
        private void OnDestroy()
        {
            if (!TryGetComponent(out ITweenCallback tween)) return;
            tween.OnTweenCompleted -= onTweenCompleted.Invoke;
            tween.OnTweenStatusChanged -= onTweenStatusChanged.Invoke;
            tween.OnCompleted -= onAnimationCompleted.Invoke;
        }
    }
}