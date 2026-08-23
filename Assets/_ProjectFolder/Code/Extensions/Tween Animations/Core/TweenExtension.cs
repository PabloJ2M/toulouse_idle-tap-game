using System;

namespace UnityEngine.Animations
{
    [Flags] public enum Direction { Up = 1, Down = 2, Left = 4, Right = 8 }
    
    public static class TweenExtension
    {
        public static Vector3 Get(this Direction direction)
        {
            var result = Vector3.zero;
            
            if ((direction & Direction.Up) != 0) result.y = 1;
            if ((direction & Direction.Down) != 0) result.y = -1;
            if ((direction & Direction.Left) != 0) result.x = -1;
            if ((direction & Direction.Right) != 0) result.x = 1;

            return result;
        }
        public static Vector3 Get(this Axis axis)
        {
            var result = Vector3.zero;

            if ((axis & Axis.X) != 0) result.x = 1;
            if ((axis & Axis.Y) != 0) result.y = 1;
            if ((axis & Axis.Z) != 0) result.z = 1;

            return result;
        }
        public static Vector3 GetInverse(this Axis axis)
        {
            var result = Vector3.one;
            
            if ((axis & Axis.X) != 0) result.x = 0;
            if ((axis & Axis.Y) != 0) result.y = 0;
            if ((axis & Axis.Z) != 0) result.z = 0;
            
            return result;
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 t)
        {
            var ab = b - a;
            var at = t - a;
            return Mathf.Clamp(Vector3.Dot(at, ab) / Vector3.Dot(ab, ab), 0f, 1f);
        }
    }
}