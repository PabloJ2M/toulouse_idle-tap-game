using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public abstract class ListPropertyDrawer : PropertyDrawer
{
    private ReorderableList _reorderableList;
    private SerializedProperty _propertyRoot;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        TryBuildList(property, label);
        _reorderableList.DoList(position);
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        TryBuildList(property, label);
        return _reorderableList.GetHeight();
    }

    private void TryBuildList(SerializedProperty property, GUIContent label)
    {
        if (_reorderableList != null && _propertyRoot != null && _propertyRoot.serializedObject == property.serializedObject &&
            _propertyRoot.propertyPath == property.propertyPath) return;

        BuildList(property, label);

        if (_reorderableList != null) {
            _reorderableList.drawElementCallback = OnDrawElement;
            _reorderableList.onAddCallback = OnAddCallback;
            _reorderableList.onRemoveCallback = OnRemoveCallback;
            _reorderableList.onCanRemoveCallback = list => list.count > 0;
            _reorderableList.onReorderCallbackWithDetails = OnReorder;
        }
        
        _propertyRoot = property;
    }

    protected void CreateList(SerializedProperty propertyList, GUIContent label)
    {
        _reorderableList = new(propertyList.serializedObject, propertyList, true, true, true, true) {
            drawHeaderCallback = rect => EditorGUI.LabelField(rect, label)
        };
    }
    protected static void ResetValue(SerializedProperty property)
    {
        var @object = property.boxedValue;
        if (@object == null) return;
        
        var type = @object.GetType();
        property.boxedValue = type.IsValueType ? Activator.CreateInstance(type) : null;
    }
    
    protected abstract void BuildList(SerializedProperty propertyRoot, GUIContent label);
    
    protected abstract void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused);
    protected abstract void OnAddCallback(ReorderableList list);
    protected abstract void OnRemoveCallback(ReorderableList list);
    protected abstract void OnReorder(ReorderableList list, int oldIndex, int newIndex);
}