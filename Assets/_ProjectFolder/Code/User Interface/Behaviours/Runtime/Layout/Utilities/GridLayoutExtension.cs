using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public struct GridCells
    {
        public int columns;
        public int rows;
    }
    public struct RectCache
    {
        public RectTransform rect;
        public Image image;
        public Sprite sprite;
        public float aspect;
    }

    public enum LayoutOrientation
    {
        Horizontal,
        Vertical
    }

    public static class GridLayoutExtension
    {
        public static void FixedArrayCapacity<T>(ref T[] array, int count)
        {
            if (array.Length < count)
                Array.Resize(ref array, count);
        }
        public static bool HasSpriteChanged(this RectCache[] cache, int length)
        {
            for (int i = 0; i < length; i++)
            {
                var c = cache[i];
                if (c.image && c.sprite != c.image.sprite)
                    return true;
            }

            return false;
        }

        public static int GetChildrenHashCode(this List<RectTransform> rectChildren)
        {
            int hash = 17;
            int count = rectChildren.Count;

            for (int i = 0; i < count; i++)
                hash = hash * 31 + rectChildren[i].GetInstanceID();

            return hash;
        }
    }
}