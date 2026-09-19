using System;

namespace UnityEngine.UI
{
    [Serializable]
    public class VirtualHorizontalGrid : VirtualHorizontal
    {
        [SerializeField] private int rows;
        
        public override void CalculateLayout(int count, Func<int, float> getDynamicSize) => 
            base.CalculateLayout(Mathf.CeilToInt((float)count / rows), getDynamicSize);
        public override void ComputeVisibleRange(float scrollPos, float viewportSize, int dataCount, int buffer, out int start, out int end)
        {
            var totalColumns = Mathf.CeilToInt((float)dataCount / rows);
            base.ComputeVisibleRange(scrollPos, viewportSize, totalColumns, buffer,
                out var firstCol, out var lastCol);
 
            start = Mathf.Max(0, firstCol * rows);
            end = Mathf.Min((lastCol + 1) * rows - 1, dataCount - 1);
        }
        public override void ApplyLayout(RectTransform rectTransform, int index)
        {
            var col = index / rows;
            var row = index % rows;
 
            var posX = GetPosition(col);
            var posY = row * (ItemSize + Spacing);
 
            rectTransform.anchorMin = new(0f, 1f);
            rectTransform.anchorMax = new(0f, 1f);
            rectTransform.pivot = new(0f, 1f);
 
            rectTransform.anchoredPosition = new(posX, -posY);
            rectTransform.sizeDelta = new(GetSize(col), ItemSize);
        }
    }
}