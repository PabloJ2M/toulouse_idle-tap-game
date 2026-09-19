using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(HOVLayoutGroupSmooth), true)]
    [CanEditMultipleObjects]
    public class HOVLayoutGroupSmoothEditor : HorizontalOrVerticalLayoutGroupEditor
    {
        private SerializedProperty _curve, _duration;

        protected override void OnEnable()
        {
            base.OnEnable();
            _curve = serializedObject.FindProperty("_curve");
            _duration = serializedObject.FindProperty("_duration");
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
            
            serializedObject.Update();

            EditorGUILayout.PropertyField(_curve, true);
            EditorGUILayout.PropertyField(_duration, true);
            
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
            
            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}