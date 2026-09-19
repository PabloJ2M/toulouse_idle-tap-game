using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class VirtualLayoutHandler
    {
        private readonly List<float> _positions = new();
        private readonly List<float> _sizes = new();

        public float TotalSize { get; private set; }

        public void CalculateLayout(int count, float defaultSize, float spacing, Func<int, float> getSizeFunc)
        {
            var currentPos = 0f;
            _positions.Clear();
            _sizes.Clear();

            for (var i = 0; i < count; i++) {
                var size = getSizeFunc?.Invoke(i) ?? defaultSize;
                _positions.Add(currentPos);
                _sizes.Add(size);
                
                currentPos += size + spacing;
            }

            TotalSize = count > 0 ? currentPos - spacing : 0;
        }
        public int GetIndexAtPosition(float scrollPos)
        {
            if (_positions.Count == 0) return 0;
            
            int low = 0, high = _positions.Count - 1;
            
            while (low <= high) {
                var mid = (low + high) / 2;
                if (_positions[mid] <= scrollPos) low = mid + 1;
                else high = mid - 1;
            }

            return Mathf.Max(0, low - 1);
        }

        public float GetPosition(int index) => _positions[index];
        public float GetSize(int index) => _sizes[index];
    }
}