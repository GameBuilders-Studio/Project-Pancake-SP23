using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace GameBuilders.Variables
{
    public class VariableReferenceDrawer<T> : PropertyDrawer
    {
        private readonly GUIContent _localContent = new("Use Constant");
        private readonly GUIContent _globalContent = new("Use Shared Variable");
        private readonly GUIStyle _buttonStyle = new()
        {
            imagePosition = ImagePosition.ImageOnly,
            padding = new RectOffset(0, 0, 0, 0),
            border = new RectOffset(0, 0, 0, 0)
        };

        private const string UsingGlobalFieldName = "UseGlobal";
        private const string GlobalVariableFieldName = "Variable";
        private const string LocalValueFieldName = "LocalValue";

        public override void OnGUI(Rect fieldRect, SerializedProperty serializedVariableReference, GUIContent label)
        {
            // Get properties
            SerializedProperty usingGlobal = serializedVariableReference.FindPropertyRelative(UsingGlobalFieldName);
            SerializedProperty globalReference = serializedVariableReference.FindPropertyRelative(GlobalVariableFieldName);
            SerializedProperty localValue = serializedVariableReference.FindPropertyRelative(LocalValueFieldName);

            label = EditorGUI.BeginProperty(fieldRect, label, serializedVariableReference);

            // Context Menu Button position
            Rect contextMenuRect = fieldRect;
            contextMenuRect.xMin = fieldRect.xMax - 18;

            // give the context button space
            fieldRect.xMax -= 24;

            EditorGUI.BeginChangeCheck();

            // Draw the property editor
            if (usingGlobal.boolValue)
            {
                var labelPosition = new Rect(fieldRect.x, fieldRect.y, fieldRect.width, fieldRect.height);

                fieldRect = EditorGUI.PrefixLabel(
                    labelPosition,
                    GUIUtility.GetControlID(FocusType.Passive),
                    label
                );

                int indent = EditorGUI.indentLevel;

                float widthSize = fieldRect.width * 0.25f;
                float offsetSize = 4.0f;

                Rect pos1 = new(fieldRect.x, fieldRect.y, widthSize - offsetSize, fieldRect.height);
                Rect pos2 = new(fieldRect.x + widthSize, fieldRect.y, 3f * widthSize, fieldRect.height);

                EditorGUI.ObjectField(pos2, globalReference, typeof(T), GUIContent.none);

                if (globalReference != null && globalReference.propertyType == SerializedPropertyType.ObjectReference && globalReference.objectReferenceValue != null)
                {
                    var data = (ScriptableObject)globalReference.objectReferenceValue;
                    var serializedObject = new SerializedObject(data);
                    SerializedProperty prop = serializedObject.FindProperty("Value");

                    EditorGUI.PropertyField(pos1, prop, GUIContent.none);

                    if (GUI.changed)
                    {
                        serializedObject.ApplyModifiedProperties();
                    }

                    serializedObject.Dispose();
                }

                EditorGUI.indentLevel = indent;
            }
            else
            {
                EditorGUI.PropertyField(fieldRect, localValue, label, true);
            }

            EditorGUI.EndProperty();

            // For the context menu, we need to save the current state of the SerializedProperty
            // iterator in 'serializedVariableReference', as the execution of the menu items will be done at a different
            // time and 'serializedVariableReference' might not point to the intended property anymore.
            SerializedProperty propertyCopy = serializedVariableReference.Copy();

            if (GUI.Button(contextMenuRect, EditorGUIUtility.IconContent("_Popup"), _buttonStyle))
            {
                var menu = new GenericMenu();
                menu.AddItem(_localContent, !usingGlobal.boolValue, () => SetUseGlobal(propertyCopy, false));
                menu.AddItem(_globalContent, usingGlobal.boolValue, () => SetUseGlobal(propertyCopy, true));
                menu.ShowAsContext();
            }
        }

        public void SetUseGlobal(SerializedProperty property, bool value)
        {
            SerializedProperty usingGlobal = property.FindPropertyRelative(UsingGlobalFieldName);
            bool currentValue = usingGlobal.boolValue;

            if (currentValue != value)
            {
                usingGlobal.boolValue = value;
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0.0f;
            SerializedProperty value;

            if (property.FindPropertyRelative(UsingGlobalFieldName).boolValue)
            {
                value = property.FindPropertyRelative(GlobalVariableFieldName);
            }
            else
            {
                value = property.FindPropertyRelative(LocalValueFieldName);
            }

            height += EditorGUI.GetPropertyHeight(value, value.isExpanded);

            return height;
        }
    }

}