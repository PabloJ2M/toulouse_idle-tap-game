namespace UnityEngine.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class RenderTextureFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RenderTexture targetRenderTexture;

        private void Awake() => rectTransform ??= transform as RectTransform;
        private void Reset() => rectTransform = transform as RectTransform;
        private void OnEnable() => FixResolution();

        [ContextMenu("Resize Texture")]
        private void OnRectTransformDimensionsChange() => FixResolution();
        
        private void FixResolution()
        {
            if (!targetRenderTexture || !rectTransform) return;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            var nuevoAncho = Mathf.Max(1, Mathf.RoundToInt(rectTransform.rect.width));
            var nuevoAlto = Mathf.Max(1, Mathf.RoundToInt(rectTransform.rect.height));
            
            if (targetRenderTexture.width == nuevoAncho && targetRenderTexture.height == nuevoAlto) 
                return;

            if (targetRenderTexture.IsCreated())
                targetRenderTexture.Release();

            targetRenderTexture.width = nuevoAncho;
            targetRenderTexture.height = nuevoAlto;
            targetRenderTexture.Create();

            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(targetRenderTexture);
            }
            #endif
        }
    }
}