namespace UnityEngine.UI
{
    public abstract class HOVLayoutGroupFitter : HorizontalOrVerticalLayoutGroup
    {
        protected RectCache[] _cache = new RectCache[10];
        protected float[] _rectSizes = new float[10];
        
        private int _childrenHashCode;
        private bool _cacheDirty;

        protected int ChildCount => rectChildren.Count;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            ValidateCache();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            _cacheDirty = true;
        }

        private void ValidateCache()
        {
            int hash = rectChildren.GetChildrenHashCode();
            if (!_cacheDirty && hash == _childrenHashCode && !_cache.HasSpriteChanged(ChildCount)) return;

            _childrenHashCode = hash;
            _cacheDirty = false;
            RebuildCache();
        }
        private void RebuildCache()
        {
            int count = ChildCount;
            GridLayoutExtension.FixedArrayCapacity(ref _cache, count);

            for (int i = 0; i < count; i++)
            {
                RectTransform rect = rectChildren[i];
                Image image = null;
                Sprite sprite = null;
                float aspect = 1f;

                if (rect.TryGetComponent(out image) && image.sprite)
                {
                    sprite = image.sprite;
                    var r = sprite.rect;
                    aspect = r.width / r.height;
                }

                _cache[i] = new() { rect = rect, aspect = aspect, image = image, sprite = sprite };
            }
        }

        protected RectTransform GetChild(int index) => _cache[index].rect;
        protected float GetHeightFromWidth(int index, float width) => width / _cache[index].aspect;
        protected float GetWidthFromHeight(int index, float height) => height * _cache[index].aspect;
    }
}