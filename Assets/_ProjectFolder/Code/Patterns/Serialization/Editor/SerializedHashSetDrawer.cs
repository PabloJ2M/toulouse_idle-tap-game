using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializedHashSet<>), true)]
public class SerializedHashSetDrawer : ListPropertyDrawer
{
    private const string PropertyName = "items";
    
    private SerializedProperty _property;

    protected override void BuildList(SerializedProperty propertyRoot, GUIContent label)
    {
        _property = propertyRoot.FindPropertyRelative(PropertyName);
        
        CreateList(_property, label);
    }

    protected override void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var propertyElement = _property.GetArrayElementAtIndex(index);
        rect.y += 2;
        rect.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(rect, propertyElement, GUIContent.none);
    }
    protected override void OnAddCallback(ReorderableList list)
    {
        int index = list.serializedProperty.arraySize;
        list.serializedProperty.arraySize++;
        list.index = index;

        ResetValue(list.serializedProperty.GetArrayElementAtIndex(index));
        list.serializedProperty.serializedObject.ApplyModifiedProperties();
    }
    protected override void OnRemoveCallback(ReorderableList list)
    {
        int index = (list.index >= 0 && list.index < list.count) ? list.index : list.count - 1;
        list.serializedProperty.DeleteArrayElementAtIndex(index);
        list.index = Mathf.Clamp(index - 1, -1, list.count - 1);
        list.serializedProperty.serializedObject.ApplyModifiedProperties();
    }
    protected override void OnReorder(ReorderableList list, int oldIndex, int newIndex) { }
}