using System.Collections.Generic;

namespace UnityEngine.InputSystem
{
    using EventSystems;
    
    public static class InputUtils
    {
        private static readonly List<RaycastResult> RaycastResults = new();
        private static readonly PointerEventData EventData = new(null);

        private static void RefreshUIElementsNonAlloc(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return;
            
            EventData.position = screenPosition;
            RaycastResults.Clear();
            
            EventSystem.current.RaycastAll(EventData, RaycastResults);
        }
        
        public static IReadOnlyList<RaycastResult> GetAllUIElements(Vector2 screenPosition)
        {
            RefreshUIElementsNonAlloc(screenPosition);
            return RaycastResults.ToArray();
        }
        public static GameObject GetTopUIElement(Vector2 screenPosition)
        {
            RefreshUIElementsNonAlloc(screenPosition);
            return RaycastResults.Count > 0 ? RaycastResults[0].gameObject : null;
        }
        
        public static bool IsPointerOverUI()
        {
            Vector2 screenPos = GetPointerScreenPosition();
            return IsPointerOverUI(screenPos);
        }
        public static bool IsPointerOverUI(Vector2 screenPosition)
        {
            RefreshUIElementsNonAlloc(screenPosition);
            return RaycastResults.Count > 0;
        }
        public static bool IsPointerOverUI(Vector2 screenPosition, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            
            RefreshUIElementsNonAlloc(screenPosition);
            
            foreach (var result in RaycastResults) {
                if (result.gameObject.CompareTag(tag))
                    return true;
            }
            
            return false;
        }
        public static bool IsPointerOverUI(Vector2 screenPosition, GameObject target)
        {
            if (target == null) return false;
 
            RefreshUIElementsNonAlloc(screenPosition);
            
            foreach (var result in RaycastResults) {
                if (result.gameObject == target || result.gameObject.transform.IsChildOf(target.transform))
                    return true;
            }
            
            return false;
        }
        
        public static Vector2 GetPointerScreenPosition()
        {
            if (Pointer.current != null)
                return Pointer.current.position.ReadValue();
 
            return Vector2.zero;
        }
    }
}