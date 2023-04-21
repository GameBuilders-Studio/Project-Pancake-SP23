using UnityEditor;
using UnityEngine;

namespace GameBuilders.Variables.Editor
{
    [CustomPropertyDrawer(typeof(VariableReference), true)]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        private readonly GUIContent _localContent = new GUIContent("Use Constant");
        private readonly GUIContent _globalContent = new GUIContent("Use Shared Variable");
        private readonly GUIStyle _buttonStyle = new GUIStyle
        {
            imagePosition = ImagePosition.ImageOnly,
            padding = new RectOffset(0, 0, 0, 0),
            border = new RectOffset(0, 0, 0, 0)
        };

        private const string UsingGlobalFieldName = "UseGlobal";
        private const string SharedVariableFieldName = "Variable";
        private const string LocalValueFieldName = "LocalValue";

        public override void OnGUI(Rect fieldRect, SerializedProperty serializedVariableReference, GUIContent label)
        {
            // In case property is an array or list element, we need to convert the property path
            // from C++ land to C#-land
            string pathToUse = ReflectionUtility.FromCppToCsPath(serializedVariableReference.propertyPath);
            VariableReference varRef = ReflectionUtility.FindFieldByPath<VariableReference>(serializedVariableReference.serializedObject.targetObject, pathToUse);

            // Get properties
            SerializedProperty usingGlobal = serializedVariableReference.FindPropertyRelative(UsingGlobalFieldName);
            SerializedProperty globalReference = serializedVariableReference.FindPropertyRelative(SharedVariableFieldName);
            SerializedProperty localValue = serializedVariableReference.FindPropertyRelative(LocalValueFieldName);

            label = EditorGUI.BeginProperty(fieldRect, label, serializedVariableReference);

            // Context Menu Button position
            Rect contextMenuRect = fieldRect;
            contextMenuRect.xMin = fieldRect.xMax - 18;

            // give the context button space
            fieldRect.xMax -= 24;

            // Draw the property editor
            if (usingGlobal.boolValue)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUI.PropertyField(fieldRect, globalReference, label, true);

                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    EditorApplication.delayCall += varRef.RaiseChangedEvent;
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();

                EditorGUI.PropertyField(fieldRect, localValue, label, true);

                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    EditorApplication.delayCall += varRef.RaiseChangedEvent;
                }
            }

            EditorGUI.EndProperty();

            // For the context menu, we need to save the current state of the SerializedProperty
            // iterator in 'serializedVariableReference', as the execution of the menu items will be done at a different
            // time and 'serializedVariableReference' might not point to the intended property anymore.
            SerializedProperty propertyCopy = serializedVariableReference.Copy();

            if (GUI.Button(contextMenuRect, EditorGUIUtility.IconContent("_Popup"), _buttonStyle))
            {
                GenericMenu menu = new();
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
            SerializedProperty value;

            if (property.FindPropertyRelative(UsingGlobalFieldName).boolValue)
            {
                value = property.FindPropertyRelative(SharedVariableFieldName);
            }
            else
            {
                value = property.FindPropertyRelative(LocalValueFieldName);
            }

            float height = EditorGUI.GetPropertyHeight(value, value.isExpanded) + EditorGUIUtility.standardVerticalSpacing;

            return height;
        }
    }

}