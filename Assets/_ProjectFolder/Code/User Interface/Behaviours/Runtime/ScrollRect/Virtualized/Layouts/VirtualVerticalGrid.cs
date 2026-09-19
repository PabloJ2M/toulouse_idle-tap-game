using System;

namespace UnityEngine.UI
{
    [Serializable]
    public class VirtualVerticalGrid : VirtualVertical
    {
        [SerializeField] private int columns;
        
        public override void CalculateLayout(int count, Func<int, float> getDynamicSize) =>
            base.CalculateLayout(Mathf.CeilToInt((float)count / columns), getDynamicSize);
        public override void ComputeVisibleRange(float scrollPos, float viewportSize, int dataCount, int buffer, out int start, out int end)
        {
            var totalRows = Mathf.CeilToInt((float)dataCount / columns);
            base.ComputeVisibleRange(scrollPos, viewportSize, totalRows, buffer, out var firstRow, out var lastRow);
 
            start = Mathf.Max(0, firstRow * columns);
            end = Mathf.Min((lastRow + 1) * columns - 1, dataCount - 1);
        }
        public override void ApplyLayout(RectTransform rt, int index)
        {
            var row = index / columns;
            var col = index % columns;
 
            var posY = GetPosition(row);
            var posX = col * (ItemSize + Spacing);
 
            rt.anchorMin = new(0f, 1f);
            rt.anchorMax = new(0f, 1f);
            rt.pivot = new(0f, 1f);
 
            rt.anchoredPosition = new(posX, -posY);
            rt.sizeDelta = new(ItemSize, GetSize(row));
        }
    }
}