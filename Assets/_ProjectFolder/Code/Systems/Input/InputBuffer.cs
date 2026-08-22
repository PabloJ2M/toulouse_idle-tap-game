using System;

namespace UnityEngine.InputSystem
{
    [Serializable]
    public class InputBuffer : InputBuffer<bool>
    {
        public void Set() => Set(true);
        public bool TryConsume() => TryConsume(out _);
    }

    [Serializable] public class IntInputBuffer : InputBuffer<int> {}
    [Serializable] public class FloatInputBuffer :  InputBuffer<float> {}
    [Serializable] public class Vector2InputBuffer :  InputBuffer<Vector2> {}
    
    [Serializable]
    public class InputBuffer<T>
    {
        [SerializeField, Min(0f)] private float holdTime = 0.1f;
        [SerializeField] private T bufferedValue;

        private float _expiryTime = float.NegativeInfinity;
        private bool _hasBufferedInput;
        
        public bool HasBuffer => _hasBufferedInput && Time.time <= _expiryTime;
        public float HoldTime => holdTime;
        public T BufferedValue => bufferedValue;

        public void Set(T value)
        {
            bufferedValue = value;
            _hasBufferedInput = true;
            _expiryTime = Time.time + holdTime;
        }
        // public void SetHoldTime(float value) => holdTime = Mathf.Max(0f, value);

        public void Consume()
        {
            bufferedValue = default;
            _hasBufferedInput = false;
            _expiryTime = float.NegativeInfinity;
        }
        public bool TryConsume(out T value)
        {
            if (!HasBuffer) {
                value = default;
                return false;
            }
            
            value = bufferedValue;
            Consume();
            return true;
        }
    }
}