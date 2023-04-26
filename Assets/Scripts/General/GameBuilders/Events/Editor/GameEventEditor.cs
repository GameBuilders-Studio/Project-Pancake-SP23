using UnityEditor;
using UnityEngine;

namespace GameBuilders.Events
{
    [CustomEditor(typeof(GameEvent), true)]
    public class EventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            GameEvent ev = target as GameEvent;

            var listeners = ev.GetListeners();

            GUILayout.Space(15);

            GUILayout.Label($"{listeners.Count} active listeners");

            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                var listener = listeners[i];

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(GUIContent.none, listener, typeof(GameEventListener), false);

                if (GUILayout.Button("Raise"))
                {
                    listener.OnEventRaised();
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(15);

            if (GUILayout.Button("Raise All"))
            {
                ev.Raise();
            }
        }
    }
}