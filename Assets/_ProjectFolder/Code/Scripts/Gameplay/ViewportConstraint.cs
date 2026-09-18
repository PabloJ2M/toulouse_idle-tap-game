namespace UnityEngine.Animations
{
    public class ViewportConstraint : MonoBehaviour
    {
        [SerializeField] private Camera cameraTarget;
        [SerializeField] private Axis snapAxis = Axis.None;
        [SerializeField] private Vector2 screen = new(0.5f, 0.5f);

        private void Awake() => cameraTarget ??= Camera.main;
        private void Reset() => cameraTarget = Camera.main;
        private void Start() => RefreshPosition();
        private void OnValidate()
        {
            screen.x = Mathf.Clamp01(screen.x);
            screen.y = Mathf.Clamp01(screen.y);
        }
        
        [ContextMenu("Refresh Position")]
        private void RefreshPosition()
        {
            var position = cameraTarget.ViewportToWorldPoint(screen);
            transform.position = snapAxis.Get(transform.position, position);
        }
    }
}