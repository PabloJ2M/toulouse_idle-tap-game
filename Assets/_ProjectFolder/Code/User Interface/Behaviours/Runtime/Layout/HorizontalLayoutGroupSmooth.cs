namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Horizontal Layout Group Smooth")]
    public class HorizontalLayoutGroupSmooth : HOVLayoutGroupSmooth
    {
        public override void CalculateLayoutInputVertical() => CalcAlongAxis(1, false);
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, false);
        }

        public override void SetLayoutHorizontal()
        {
            if (!Application.isPlaying)
            {
                SetChildrenAlongAxis(0, false);
                return;
            }

            Cache();
            SaveOriginPositions();
            SetChildrenAlongAxis(0, false);
        }
        public override void SetLayoutVertical()
        {
            if (!Application.isPlaying)
            {
                SetChildrenAlongAxis(1, false);
                return;
            }

            SetChildrenAlongAxis(1, false);
            CaptureTargetPositions();
        }
    }
}