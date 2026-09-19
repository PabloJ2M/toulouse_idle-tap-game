namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Grid Layout Group Flexible")]
    public class GridLayoutGroupFlexible : LayoutGroup
    {
        [SerializeField] private Vector2 _cellSize = new(120, 120);
        [SerializeField] private Vector2 _spacing;

        public override void CalculateLayoutInputHorizontal() => base.CalculateLayoutInputHorizontal();
        public override void CalculateLayoutInputVertical() { }
        public override void SetLayoutHorizontal() => Layout();
        public override void SetLayoutVertical() { }

        private void Layout()
        {
            float width = rectTransform.rect.width - padding.left - padding.right;

            float fullCell = _cellSize.x + _spacing.x;
            int columns = Mathf.Max(1, Mathf.FloorToInt((width + _spacing.x) / fullCell));

            float[] columnHeights = new float[columns];
            int count = rectChildren.Count;

            int[] spansX = new int[count];
            int[] spansY = new int[count];

            for (int i = 0; i < count; i++)
            {
                LayoutElement element = rectChildren[i].GetComponent<LayoutElement>();

                int spanX = 1;
                int spanY = 1;

                if (element)
                {
                    if (element.flexibleWidth > 1)
                        spanX = Mathf.Clamp((int)element.flexibleWidth, 1, columns);

                    if (element.flexibleHeight > 1)
                        spanY = Mathf.Max((int)element.flexibleHeight, 1);
                }

                spansX[i] = spanX;
                spansY[i] = spanY;
            }

            float[] posX = new float[count];
            float[] posY = new float[count];
            float[] widthSpan = new float[count];
            float[] heightSpan = new float[count];

            for (int i = 0; i < count; i++)
            {
                int spanX = spansX[i];
                int spanY = spansY[i];

                float w = spanX * _cellSize.x + (spanX - 1) * _spacing.x;
                float h = spanY * _cellSize.y + (spanY - 1) * _spacing.y;

                int column = FindBestColumn(columnHeights, spanX);

                float y = GetMaxHeight(columnHeights, column, spanX);

                posX[i] = column * (_cellSize.x + _spacing.x);
                posY[i] = y;

                widthSpan[i] = w;
                heightSpan[i] = h;

                float newHeight = y + h + _spacing.y;

                for (int c = column; c < column + spanX; c++)
                    columnHeights[c] = newHeight;
            }

            float contentHeight = 0;
            for (int i = 0; i < columns; i++)
                contentHeight = Mathf.Max(contentHeight, columnHeights[i]);

            float contentWidth = columns * _cellSize.x + (columns - 1) * _spacing.x;

            contentWidth += padding.left + padding.right;
            contentHeight += padding.top + padding.bottom;

            float offsetX = GetStartOffset(0, contentWidth);
            float offsetY = GetStartOffset(1, contentHeight);

            for (int i = 0; i < count; i++)
            {
                float x = padding.left + posX[i] + offsetX;
                float y = padding.top + posY[i] + offsetY;

                SetChildAlongAxis(rectChildren[i], 0, x, widthSpan[i]);
                SetChildAlongAxis(rectChildren[i], 1, y, heightSpan[i]);
            }

            SetLayoutInputForAxis(contentWidth, contentWidth, -1, 0);
            SetLayoutInputForAxis(contentHeight, contentHeight, -1, 1);
        }
        private int FindBestColumn(float[] heights, int span)
        {
            int bestColumn = 0;
            float bestHeight = float.MaxValue;

            for (int i = 0; i <= heights.Length - span; i++)
            {
                float h = GetMaxHeight(heights, i, span);

                if (h < bestHeight)
                {
                    bestHeight = h;
                    bestColumn = i;
                }
            }

            return bestColumn;
        }
        private float GetMaxHeight(float[] heights, int start, int span)
        {
            float h = heights[start];

            for (int i = 1; i < span; i++)
                h = Mathf.Max(h, heights[start + i]);

            return h;
        }
    }
}