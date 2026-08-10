using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializedDictionary<,>), true)]
public class SerializedDictionaryDrawer : ListPropertyDrawer
{
    private const string KeysPropertyName = "keys";
    private const string ValuesPropertyName = "values";
    
    private SerializedProperty _keysProperty;
    private SerializedProperty _valuesProperty;

    protected override void BuildList(SerializedProperty rootProperty, GUIContent label)
    {
        _keysProperty = rootProperty.FindPropertyRelative(KeysPropertyName);
        _valuesProperty = rootProperty.FindPropertyRelative(ValuesPropertyName);
        
        CreateList(_keysProperty, label);
    }

    protected override void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        float half = rect.width / 2f - 4f;
        rect.y += 2;
        rect.height = EditorGUIUtility.singleLineHeight;

        var keyRect = new Rect(rect.x, rect.y, half, rect.height);
        var valueRect = new Rect(rect.x + half + 8f, rect.y, half, rect.height);

        EditorGUI.PropertyField(keyRect, _keysProperty.GetArrayElementAtIndex(index), GUIContent.none);
        EditorGUI.PropertyField(valueRect, _valuesProperty.GetArrayElementAtIndex(index), GUIContent.none);
    }
    protected override void OnAddCallback(ReorderableList list)
    {
        int index = _keysProperty.arraySize;
        _keysProperty.arraySize++;
        _valuesProperty.arraySize++;
        list.index = index;

        ResetValue(_keysProperty.GetArrayElementAtIndex(index));
        ResetValue(_valuesProperty.GetArrayElementAtIndex(index));

        _keysProperty.serializedObject.ApplyModifiedProperties();
    }
    protected override void OnRemoveCallback(ReorderableList list)
    {
        int index = (list.index >= 0 && list.index < list.count) ? list.index : list.count - 1;

        _keysProperty.DeleteArrayElementAtIndex(index);
        _valuesProperty.DeleteArrayElementAtIndex(index);

        list.index = Mathf.Clamp(index - 1, -1, list.count - 1);
        _keysProperty.serializedObject.ApplyModifiedProperties();
    }
    protected override void OnReorder(ReorderableList list, int oldIndex, int newIndex)
    {
        _valuesProperty.MoveArrayElement(oldIndex, newIndex);
        _valuesProperty.serializedObject.ApplyModifiedProperties();
    }
}