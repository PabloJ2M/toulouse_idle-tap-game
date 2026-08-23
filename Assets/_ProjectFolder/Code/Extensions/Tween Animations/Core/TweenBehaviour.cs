using PrimeTween;

namespace UnityEngine.Animations
{
    [RequireComponent(typeof(TweenCore))]
    public abstract class TweenBehaviour<T> : MonoBehaviour where T : struct
    {
        protected ITween TweenCore;
        protected Tween Tween;
        
        protected TweenSettings Settings => TweenCore.Settings;
        protected TweenSettings<T> TweenSettings;
        
        private ITweenCallback _tweenCallback;

        protected virtual void Awake()
        {
            TweenCore = GetComponent<ITween>();
            _tweenCallback = GetComponent<ITweenCallback>();

            _tweenCallback.OnTweenStatusChanged += OnPlay;
            _tweenCallback.OnDisabled += OnCancel;
            _tweenCallback.OnEnabled += OnStart;
        }
        protected virtual void OnDestroy()
        {
            _tweenCallback.OnTweenStatusChanged -= OnPlay;
            _tweenCallback.OnDisabled -= OnCancel;
            _tweenCallback.OnEnabled -= OnStart;
        }

        protected virtual void OnPlay(bool value)
        {
            if (Tween.isAlive)
                OnCancel();
        }
        protected virtual void OnStart() { }
        protected virtual void OnCancel() => Tween.Stop();
        protected virtual void OnComplete() => _tweenCallback?.OnComplete();
    }
}