using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    using Pool;

    [RequireComponent(typeof(ScrollRect))]
    public abstract class VirtualScrollBehaviour<T> : PoolBehaviour
    {
        [SerializeField] protected float spacing;
        [SerializeField] private int buffer = 2;

        [SerializeReference] private IVirtualLayout layout;

        private ScrollRect _scroll;
        private IList<T> _data;

        private readonly Dictionary<int, VirtualObject> _activeItems = new();
        private int _firstVisible = -1, _lastVisible = -1;

        protected override void Awake()
        {
            base.Awake();
            layout??= CreateLayout();
            _scroll = GetComponent<ScrollRect>();

            if (poolSettings.Prefab is VirtualObject obj)
                layout?.Initialize(obj.RectTransform, spacing);
        }
        protected virtual void Reset() => layout = CreateLayout();
        protected virtual void OnEnable() => _scroll.onValueChanged.AddListener(_ => OnUpdateScroll());
        protected virtual void OnDisable() => _scroll.onValueChanged.RemoveListener(_ => OnUpdateScroll());

        private void RecalculateLayout()
        {
            layout?.CalculateLayout(_data.Count, GetDynamicSize);
            layout?.UpdateContainerSize(_scroll.content);
        }
        private void ResetView()
        {
            _scroll.content.anchoredPosition = Vector2.zero;
            _firstVisible = _lastVisible = -1;
            Clear();
        }

        private void OnUpdateScroll(bool force = false)
        {
            if (layout == null || _data == null || _data.Count == 0) return;

            var scrollPos = Mathf.Abs(layout.GetScrollPosition(_scroll));
            var viewportSize = layout.GetViewportSize(_scroll);

            layout.ComputeVisibleRange(scrollPos, viewportSize, _data.Count, buffer,
                out var newFirst, out var newLast);

            if (!force && newFirst == _firstVisible && newLast == _lastVisible) return;
            
            _firstVisible = newFirst;
            _lastVisible = newLast;
            RefreshLayout();
        }
        private void RefreshLayout()
        {
            CleanOutOfBounds();
            UpdateVisibleItems();
        }
        private void CleanOutOfBounds()
        {
            var activeKeys = _activeItems.Keys
                .Where(index => index < _firstVisible || index > _lastVisible).ToList();
            
            foreach (var index in activeKeys) {
                _activeItems[index].Destroy();
                _activeItems.Remove(index);
            }
        }
        private void UpdateVisibleItems()
        {
            for (var i = _firstVisible; i <= _lastVisible; i++)
            {
                if (!_activeItems.TryGetValue(i, out var item)) {
                    item = GetPrefab() as VirtualObject;
                    BindInstanceData(item, i);
                    _activeItems.Add(i, item);
                }

                if (item)
                    layout?.ApplyLayout(item.RectTransform, i);
            }
        }

        protected abstract IVirtualLayout CreateLayout();
        protected abstract void BindInstanceData(VirtualObject item, int dataIndex);
        protected abstract float GetDynamicSize(int index);
        
        public void SetData(IList<T> values)
        {
            _data = values;
            Canvas.ForceUpdateCanvases();

            RecalculateLayout();
            ResetView();
            
            OnUpdateScroll(true);
        }
    }
}