#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using BehaviourCollections;

[CustomEditor(typeof(BehaviourSet<>), true)]
public class BehaviourSetEditor : Editor
{
    private Type _type;

    public override void OnInspectorGUI()
    {
        if (_type == null)
        {
            _type = target.GetType();

            while (!IsSubclassOfRawGeneric(typeof(BehaviourSet<>), _type))
            {
                _type = _type.BaseType;
            }

            _type = _type.BaseType;
        }

        var noDuplicates = serializedObject.FindProperty("NoDuplicates");

        if (!noDuplicates.boolValue)
        {
            string behaviourName = _type.GetGenericArguments()[0].Name;
            EditorGUILayout.HelpBox($"Multiple {behaviourName}s cannot derive from the same type", MessageType.Error);
        }

        bool previousGUIState = GUI.enabled;
        GUI.enabled = false;

        base.OnInspectorGUI();

        GUI.enabled = previousGUIState;

        if (GUILayout.Button("Refresh Behaviours"))
        {
            MethodInfo method = _type.GetTypeInfo().GetDeclaredMethod("GetBehavioursAndInterfaces");
            method?.Invoke(target, null);
        }
    }

    private bool IsSubclassOfRawGeneric(Type generic, Type toCheck) 
    {
        while (toCheck != null && toCheck != typeof(object)) 
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur) { return true; }
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}

#endif