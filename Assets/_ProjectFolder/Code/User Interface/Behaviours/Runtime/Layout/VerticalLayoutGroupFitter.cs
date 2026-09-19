namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Vertical Layout Group Fitter")]
    public class VerticalLayoutGroupFitter : HOVLayoutGroupFitter
    {
        public override void CalculateLayoutInputHorizontal() => base.CalculateLayoutInputHorizontal();
        public override void CalculateLayoutInputVertical()
        {
            float width = rectTransform.rect.width - padding.horizontal;
            float totalHeight = padding.top + padding.bottom;

            int count = ChildCount;
            GridLayoutExtension.FixedArrayCapacity(ref _rectSizes, count);

            for (int i = 0; i < count; i++)
            {
                float hight = GetHeightFromWidth(i, width);
                totalHeight += hight;
                _rectSizes[i] = hight;

                if (i < count - 1)
                    totalHeight += spacing;
            }

            SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
        }
        public override void SetLayoutHorizontal()
        {
            float width = rectTransform.rect.width - padding.horizontal;
            int count = ChildCount;

            for (int i = 0; i < count; i++)
                SetChildAlongAxis(GetChild(i), 0, padding.left, width);
        }
        public override void SetLayoutVertical()
        {
            float y = padding.top;
            int count = ChildCount;

            if (!reverseArrangement)
            {
                for (int i = 0; i < count; i++)
                    SetChildAxis(i);
            }
            else
            {
                for (int i = count - 1; i >= 0; i--)
                    SetChildAxis(i);
            }

            void SetChildAxis(int index)
            {
                float hight = _rectSizes[index];
                SetChildAlongAxis(GetChild(index), 1, y, hight);
                y += hight + spacing;
            }
        }
    }
}