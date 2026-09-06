namespace UnityEngine.Rendering
{
    public class RenderBuffer : MonoBehaviour
    {
        [SerializeField] private float size = 5;
        [SerializeField] private RenderTexture outputTexture;

        [ContextMenu("Force Update")]
        public void ForceUpdate() => DrawTexture();
        
        private void Update() => DrawTexture();
        private void DrawTexture()
        {
            var cmd = CommandBufferPool.Get();
            cmd.SetViewMatrix(GetViewMatrix());
            cmd.SetProjectionMatrix(GetProjectionMatrix());
            
            cmd.SetRenderTarget(outputTexture);
            cmd.ClearRenderTarget(true, true, Color.clear, 1f);
            
            //draw elements**
            
            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private Matrix4x4 GetViewMatrix()
        {
            var scale = new Vector3(1, 1, -1);
            return Matrix4x4.TRS(transform.position, transform.rotation, scale).inverse;
        }
        private Matrix4x4 GetProjectionMatrix()
        {
            var aspect = (float)outputTexture.width / outputTexture.height;
            var halfWidth = size * aspect;
            var proj = Matrix4x4.Ortho(-halfWidth, halfWidth, -size, size, 0.1f, 10f);
            return GL.GetGPUProjectionMatrix(proj, true);
        }
    }
}