using System;

namespace UnityEngine.UI
{
    public abstract class VirtualVertical : IVirtualLayout
    {
        private readonly VirtualLayoutHandler _layout = new();
        
        public float TotalSize => _layout.TotalSize;
        public float ItemSize { get; private set; }
        protected float Spacing { get; private set; }
        
        public virtual void Initialize(RectTransform prefab, float spacing)
        {
            Spacing = spacing;
            ItemSize = prefab.rect.height;
        }

        public virtual void CalculateLayout(int count, Func<int, float> getDynamicSize) =>
            _layout.CalculateLayout(count, ItemSize, Spacing, getDynamicSize);
        public virtual void ComputeVisibleRange(float scrollPos, float viewportSize, int dataCount, int buffer, out int start, out int end)
        {
            start = Mathf.Max(0, _layout.GetIndexAtPosition(scrollPos) - buffer);
            end = Mathf.Min(dataCount - 1, _layout.GetIndexAtPosition(scrollPos + viewportSize) + buffer);
        }
        public virtual void ApplyLayout(RectTransform rectTransform, int index)
        {
            var pos = _layout.GetPosition(index);
            var size = _layout.GetSize(index);
 
            rectTransform.anchorMin = new(0f, 1f);
            rectTransform.anchorMax = new(1f, 1f);
            rectTransform.pivot = new(0.5f, 1f);
 
            rectTransform.anchoredPosition = new(0f, -pos);
            rectTransform.sizeDelta = new(0f, size);
        }

        public void UpdateContainerSize(RectTransform content) => content.sizeDelta = new(content.sizeDelta.x, TotalSize);
        public float GetScrollPosition(ScrollRect scroll) => scroll.content.anchoredPosition.y;
        public float GetViewportSize(ScrollRect scroll) => scroll.viewport.rect.height;
        
        protected float GetPosition(int index) => _layout.GetPosition(index);
        protected float GetSize(int index) => _layout.GetSize(index);
    }
}