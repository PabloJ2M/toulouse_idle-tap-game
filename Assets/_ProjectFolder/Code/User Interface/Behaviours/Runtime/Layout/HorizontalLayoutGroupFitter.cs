namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Horizontal Layout Group Fitter")]
    public class HorizontalLayoutGroupFitter : HOVLayoutGroupFitter
    {
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            float height = rectTransform.rect.height - padding.vertical;
            float totalWidth = padding.left + padding.right;

            int count = ChildCount;
            GridLayoutExtension.FixedArrayCapacity(ref _rectSizes, count);

            for (int i = 0; i < count; i++)
            {
                float width = GetWidthFromHeight(i, height);
                totalWidth += width;
                _rectSizes[i] = width;

                if (i < count - 1)
                    totalWidth += spacing;
            }

            SetLayoutInputForAxis(totalWidth, totalWidth, -1, 0);
        }
        public override void CalculateLayoutInputVertical() { }
        public override void SetLayoutHorizontal()
        {
            float x = padding.left;
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
                float width = _rectSizes[index];
                SetChildAlongAxis(GetChild(index), 0, x, width);
                x += width + spacing;
            }
        }
        public override void SetLayoutVertical()
        {
            float height = rectTransform.rect.height - padding.vertical;
            int count = ChildCount;

            for (int i = 0; i < count; i++)
                SetChildAlongAxis(GetChild(i), 1, padding.top, height);
        }
    }
}