namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Vertical Layout Group Smooth")]
    public class VerticalLayoutGroupSmooth : HOVLayoutGroupSmooth
    {
        public override void CalculateLayoutInputVertical() => CalcAlongAxis(1, true);
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, true);
        }

        public override void SetLayoutHorizontal()
        {
            if (!Application.isPlaying)
            {
                SetChildrenAlongAxis(0, true);
                return;
            }

            Cache();
            SaveOriginPositions();
            SetChildrenAlongAxis(0, true);
        }
        public override void SetLayoutVertical()
        {
            if (!Application.isPlaying)
            {
                SetChildrenAlongAxis(1, true);
                return;
            }

            SetChildrenAlongAxis(1, true);
            CaptureTargetPositions();
        }
    }
}