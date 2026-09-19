namespace UnityEngine.UI
{
    public abstract class HOVLayoutGroupSmooth : HorizontalOrVerticalLayoutGroup
    {
        [SerializeField] protected AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] protected float _duration = 1f;

        protected Vector2[] _origin, _target;
        private bool _hasInitialized, _isAnimating;
        private float _inverseDuration, _time;

        protected void Cache()
        {
            int count = rectChildren.Count;
            if (_target != null && _target.Length == count) return;
            _target = new Vector2[count];
            _origin = new Vector2[count];
        }
        protected void SaveOriginPositions()
        {
            int count = rectChildren.Count;
            for (int i = 0; i < count; i++)
                _origin[i] = rectChildren[i].anchoredPosition;
        }
        protected void CaptureTargetPositions()
        {
            int count = rectChildren.Count;
            for(int i = 0; i < count; i++)
            {
                var rect = rectChildren[i];
                _target[i] = rect.anchoredPosition;

                if (!_hasInitialized) _origin[i] = _target[i];
                rect.anchoredPosition = _origin[i];
            }

            _time = 0f;
            _inverseDuration = 1f / _duration;
            _hasInitialized = _isAnimating = true;
        }

        protected virtual void LateUpdate()
        {
            if (!Application.isPlaying || !_isAnimating) return;
            _time += Time.deltaTime * _inverseDuration;
            _time = Mathf.Clamp01(_time);

            int count = rectChildren.Count;
            float time = _curve.Evaluate(_time);

            for (int i = 0; i < count; i++)
                rectChildren[i].anchoredPosition = Vector2.LerpUnclamped(_origin[i], _target[i], time);

            if (_time >= 1f) _isAnimating = false;
        }
    }
}