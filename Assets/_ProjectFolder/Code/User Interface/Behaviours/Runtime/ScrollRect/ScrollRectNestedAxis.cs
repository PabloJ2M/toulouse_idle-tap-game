namespace UnityEngine.UI
{
    using EventSystems;

    [AddComponentMenu("UI (Canvas)/Scroll Rect Nested")]
    public class ScrollRectNestedAxis : ScrollRect
    {
        private GameObject _parentObject;
        private bool _isParentAxis;

        protected override void Awake()
        {
            base.Awake();
            _parentObject = transform.parent.gameObject;
        }
        protected bool IsOtherAxis(PointerEventData eventData)
        {
            var delta = eventData.delta;

            if (horizontal && vertical) return false;
            else if (horizontal) return Mathf.Abs(delta.y) > Mathf.Abs(delta.x);
            else if (vertical) return Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
            return false;
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            base.OnInitializePotentialDrag(eventData);
            if (_parentObject) ExecuteEvents.ExecuteHierarchy(_parentObject, eventData, ExecuteEvents.initializePotentialDrag);
        }
        public override void OnBeginDrag(PointerEventData eventData)
        {
            _isParentAxis = _parentObject && IsOtherAxis(eventData);

            if (!_isParentAxis) base.OnBeginDrag(eventData);
            else ExecuteEvents.ExecuteHierarchy(_parentObject, eventData, ExecuteEvents.beginDragHandler);
        }
        public override void OnDrag(PointerEventData eventData)
        {
            if (!_isParentAxis) base.OnDrag(eventData);
            else ExecuteEvents.ExecuteHierarchy(_parentObject, eventData, ExecuteEvents.dragHandler);
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!_isParentAxis) base.OnEndDrag(eventData);
            else ExecuteEvents.ExecuteHierarchy(_parentObject, eventData, ExecuteEvents.endDragHandler);

            _isParentAxis = false;
        }
    }
}