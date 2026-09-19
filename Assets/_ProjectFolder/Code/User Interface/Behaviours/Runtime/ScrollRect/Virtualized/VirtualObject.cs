using UnityEngine.Pool;

namespace UnityEngine.UI
{
    public class VirtualObject : PoolObject
    {
        public RectTransform RectTransform => Transform as RectTransform;
    }
}