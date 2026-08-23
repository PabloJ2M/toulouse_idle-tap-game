using System;

namespace UnityEngine.Animations
{
    [Serializable]
    public class ScaleFactor
    {
        [SerializeField, Range(0f, 1f)] private float normal = 1f;
        [SerializeField, Range(0f, 2f)] private float scaled = 1f;
        
        public float Normal => normal;
        public float Scaled => scaled;
    }
}