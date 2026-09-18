// using UnityEngine.Rendering.Universal;

namespace UnityEngine.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class RenderTextureFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Camera renderCamera;
        [Space]
        [SerializeField] private RenderTexture targetRenderTexture;

        private void Awake() => rectTransform ??= transform as RectTransform;
        private void Reset() => rectTransform = transform as RectTransform;
        private void Start() => FixResolution();

        [ContextMenu("Resize Texture")]
        private void OnRectTransformDimensionsChange()
        {
            if (gameObject.activeInHierarchy)
                FixResolution();
        }
        
        private void FixResolution()
        {
            if (!targetRenderTexture || !rectTransform || !renderCamera) return;
            
            // LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            Canvas.ForceUpdateCanvases();
            var newWidth = Mathf.Max(1, Mathf.RoundToInt(rectTransform.rect.width));
            var newHeight = Mathf.Max(1, Mathf.RoundToInt(rectTransform.rect.height));
            
            if (targetRenderTexture.width == newWidth && targetRenderTexture.height == newHeight) 
                return;

            if (targetRenderTexture.IsCreated())
                targetRenderTexture.Release();

            targetRenderTexture.width = newWidth;
            targetRenderTexture.height = newHeight;
            targetRenderTexture.Create();
            
            renderCamera.aspect = (float)newWidth / (float)newHeight;
            UpdateEditor();
        }
        private void UpdateEditor()
        {
            #if UNITY_EDITOR
            if (Application.isPlaying) return;
            
            renderCamera.Render();
            UnityEditor.EditorUtility.SetDirty(targetRenderTexture);
            #endif
        }
    }
}