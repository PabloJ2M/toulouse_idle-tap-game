using UnityEngine;

namespace Gameplay.Environment.Parallax
{
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float speedMultiply = 1;

        private const string ID = "_TextureOffset";
        private static readonly int TextureOffset = Shader.PropertyToID(ID);
        
        private Renderer _render;
        private MaterialPropertyBlock _propertyBlock;
        private Vector2 _currentOffset;

        private void Awake()
        {
            _render = GetComponent<Renderer>();
            _propertyBlock = new();
            
            _render.GetPropertyBlock(_propertyBlock);
            _currentOffset = _propertyBlock.GetVector(TextureOffset);
        }
        public void OnUpdate(Vector2 directionOffset)
        {
            var movement = speedMultiply * directionOffset;
            if (movement == Vector2.zero) return;
            
            _currentOffset += movement;
            
            _render.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(TextureOffset, _currentOffset);
            _render.SetPropertyBlock(_propertyBlock);
        }
    }
}