using UnityEngine;

namespace Gameplay.Environment.Parallax
{
    public class ParallaxGroup : MonoBehaviour
    {
        [SerializeField] private Vector2 direction;
        [SerializeField, Range(-1f, 1f)] private float speed = 1;
        
        private ParallaxLayer[] _layers;

        private void Awake() => _layers = GetComponentsInChildren<ParallaxLayer>();
        private void FixedUpdate()
        {
            var displacement = speed * Time.deltaTime * direction;
            
            foreach (var layer in _layers)
                layer.OnUpdate(displacement);
        }
    }
}