using System;

namespace UnityEngine.UI
{
    public interface IVirtualLayout
    {
        float TotalSize { get; }
        float ItemSize { get; }
        
        /// <summary></summary>
        /// <param name="prefab">prefab reference for rect size</param>
        /// <param name="spacing">spacing between objects</param>
        void Initialize(RectTransform prefab, float spacing);
        
        /// <summary>
        /// Recalculate positions/sizes for counting elements.
        /// Index in getDynamicSize depends on the implementation:
        /// In lineal alignment is item index, on grid is row/column.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="getDynamicSize"></param>
        void CalculateLayout(int count, Func<int, float> getDynamicSize);
        void ComputeVisibleRange(float scrollPos, float viewportSize, int dataCount, int buffer, out int start, out int end);
        
        /// <summary>
        /// Set position and dimensions for item at index in pool
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="index"></param>
        void ApplyLayout(RectTransform rectTransform, int index);
        
        /// <summary>
        /// Updates scroll container size
        /// </summary>
        /// <param name="content"></param>
        void UpdateContainerSize(RectTransform content);
        
        float GetScrollPosition(ScrollRect scroll);
        float GetViewportSize(ScrollRect scroll);
    }
}