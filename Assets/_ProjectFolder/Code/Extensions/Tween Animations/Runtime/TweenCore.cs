using System;
using System.Threading;
using PrimeTween;

namespace UnityEngine.Animations
{
    [DefaultExecutionOrder(100)]    
    public class TweenCore : MonoBehaviour, ITween, ITweenCallback
    {
        [SerializeField] private TweenGroup group;
        [SerializeField] private bool startDisable, playOnAwake;
        [SerializeField] private TweenSettings settings;

        public TweenSettings Settings => settings;
        public bool IsEnabled { get; private set; }

        public event Action<bool> OnTweenStatusChanged, OnTweenCompleted;
        public event Action OnEnabled, OnDisabled;
        public event Action OnCompleted;

        private CancellationTokenSource _cts;
        
        private void Awake() => group?.AddListener(this);
        private void OnEnable() => _ = EnableTween();
        private void OnDisable() => DisableTween();
        private void OnDestroy()
        {
            _cts?.Dispose();
            group?.RemoveListener(this);
        }

        private async Awaitable EnableTween()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new();
            
            IsEnabled = !startDisable;
            OnEnabled?.Invoke();
            
            if (!playOnAwake) return;
            
            try {
                await Awaitable.NextFrameAsync(_cts.Token);
                Play(!IsEnabled);
            }
            catch (OperationCanceledException) { }
        }
        private void DisableTween()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            OnDisabled?.Invoke();
        }

        public void Play(bool value)
        {
            if (value == IsEnabled) return;

            OnTweenStatusChanged?.Invoke(value);
            IsEnabled = value;
        }
        public void ForcePlay(bool value)
        {
            IsEnabled = !value;
            Play(value);
        }
        public void OnComplete()
        {
            OnTweenCompleted?.Invoke(IsEnabled);
            OnCompleted?.Invoke();
        }
    }
}