using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Animations
{
    using Events;

    public class TweenGroup : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float timeToStart, timeInBetween;
        [SerializeField] private bool playOnAwake;

        [SerializeField] private bool inverseCallback;
        [SerializeField] private UnityEvent<bool> onValueChanged;

        private WaitForSeconds _waitToStart, _waitTimeInBetween;
        private readonly List<ITween> _tweenList = new();

        private void Awake()
        {
            _waitToStart = new(timeToStart);
            _waitTimeInBetween = new(timeInBetween);
        }
        private void OnEnable()
        {
            if (playOnAwake)
                EnableGroup();
        }

        public void AddListener(ITween tween) => _tweenList.Add(tween);
        public void RemoveListener(ITween tween) => _tweenList.Remove(tween);

        [ContextMenu("Enable Group")] public void EnableGroup() => SetGroupStatus(true);
        [ContextMenu("Disable Group")] public void DisableGroup() => SetGroupStatus(false);
        
        private void SetGroupStatus(bool value)
        {
            StopAllCoroutines();
            StartCoroutine(TweenDelay(value));
        }

        private IEnumerator TweenDelay(bool value)
        {
            yield return _waitToStart;
            onValueChanged?.Invoke(inverseCallback ? !value : value);

            foreach (var tween in _tweenList)
            {
                tween.Play(value);

                if (timeInBetween != 0)
                    yield return _waitTimeInBetween;
            }
        }
    }
}