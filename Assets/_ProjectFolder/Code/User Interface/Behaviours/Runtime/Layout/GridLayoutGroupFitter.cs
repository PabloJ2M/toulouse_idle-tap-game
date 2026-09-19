namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Grid Layout Group Fitter")]
    public class GridLayoutGroupFitter : LayoutGroup
    {
        [SerializeField] private Vector2 _cellSize = new(100f, 100f), _spacing;
        [SerializeField, Range(0.1f, 2f)] private float _minScale = 0.5f;
        [SerializeField, Range(0.1f, 2f)] private float _maxScale = 2f;
        [SerializeField] private uint _minColumns = 3;

        private Vector2 _gridCellSize;
        private GridCells _gridCells;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            UpdateGridLayout();

            float width = _gridCellSize.x * _gridCells.columns + _spacing.x * (_gridCells.columns - 1) + padding.left + padding.right;
            SetLayoutInputForAxis(width, width, -1, 0);
        }
        public override void CalculateLayoutInputVertical()
        {
            int count = rectChildren.Count;
            _gridCells.rows = Mathf.CeilToInt(count / (float)_gridCells.columns);

            float height = _gridCellSize.y * _gridCells.rows + _spacing.y * (_gridCells.rows - 1) + padding.top + padding.bottom;
            SetLayoutInputForAxis(height, height, -1, 1);
        }

        public override void SetLayoutHorizontal() => SetCells();
        public override void SetLayoutVertical() { }

        private void UpdateGridLayout()
        {
            float width = rectTransform.rect.width;
            float availableWidth = width - padding.left - padding.right;

            float minCellWidth = _cellSize.x * _minScale;
            float maxCellWidth = _cellSize.x * _maxScale;

            int columns = Mathf.FloorToInt((availableWidth + _spacing.x) / (_cellSize.x + _spacing.x));
            columns = Mathf.Max(columns, (int)_minColumns);

            float cellWidth = (availableWidth - (_spacing.x * (columns - 1))) / columns;

            float aspect = _cellSize.y / _cellSize.x;
            float cellHeight = cellWidth * aspect;

            float minCellHeight = _cellSize.y * _minScale;
            float maxCellHeight = _cellSize.y * _maxScale;

            cellWidth = Mathf.Clamp(cellWidth, minCellWidth, maxCellWidth);
            cellHeight = Mathf.Clamp(cellHeight, minCellHeight, maxCellHeight);

            _gridCellSize = new(cellWidth, cellHeight);
            _gridCells.columns = columns;
        }
        private void SetCells()
        {
            int count = rectChildren.Count;

            float stepX = _gridCellSize.x + _spacing.x;
            float stepY = _gridCellSize.y + _spacing.y;

            float contentHeight = _gridCellSize.y * _gridCells.rows + _spacing.y * (_gridCells.rows - 1);
            float startY = GetStartOffset(1, contentHeight);

            int itemsLastRow = count % _gridCells.columns;
            if (itemsLastRow == 0) itemsLastRow = _gridCells.columns;

            for (int i = 0; i < count; i++)
            {
                int row = i / _gridCells.columns;
                int column = i % _gridCells.columns;

                int itemsThisRow = (row == _gridCells.rows - 1) ? itemsLastRow : _gridCells.columns;

                float rowWidth = _gridCellSize.x * itemsThisRow + _spacing.x * (itemsThisRow - 1);
                float startX = GetStartOffset(0, rowWidth);

                float x = startX + stepX * column;
                float y = startY + stepY * row;

                SetChildAlongAxis(rectChildren[i], 0, x, _gridCellSize.x);
                SetChildAlongAxis(rectChildren[i], 1, y, _gridCellSize.y);
            }
        }
    }
}