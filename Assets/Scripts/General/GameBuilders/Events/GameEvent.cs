using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace GameBuilders.Events
{
    [CreateAssetMenu(menuName = "GameBuilders/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        [SerializeField]
        [TextArea(3, 10)]
        private string _description;

        [SerializeField]
        [Tooltip("Logs the stack trace of each Raise() call.")]
        private bool _enableStackTraceLogging = false;

        /// <summary>
        /// The list of listeners that this event will notify if it is raised.
        /// </summary>
        private readonly List<GameEventListener> _eventListeners = new();

        public void Raise()
        {
            for (int i = _eventListeners.Count - 1; i >= 0; i--)
            {
                _eventListeners[i].OnEventRaised();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_enableStackTraceLogging)
            {
                LogRaise();
            }
#endif
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (!_eventListeners.Contains(listener))
            {
                _eventListeners.Add(listener);
            }
        }

        public void UnregisterListener(GameEventListener listener)
        {
            _eventListeners.Remove(listener);
        }

        internal List<GameEventListener> GetListeners()
        {
            return _eventListeners;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void LogRaise()
        {
            StackFrame frame = new(1, true);

            var method = frame.GetMethod();
            string fileName = frame.GetFileName();
            int lineNumber = frame.GetFileLineNumber();

            string methodName = method.Name;
            string declaringType = method.DeclaringType.FullName;
            var methodParams = method.GetParameters();

            var paramSig = new StringBuilder(32);
            for (int i = 0; i < methodParams.Length; i++)
            {
                paramSig.Append($"{methodParams[i].ParameterType} {methodParams[i].Name}");
                if (i+1 < methodParams.Length)
                {
                    paramSig.Append(", ");
                }
            }

            string formattedMethodName = $"{declaringType}.{methodName}({paramSig})";

            if (!string.IsNullOrEmpty(fileName))
            {
                int startSubName = fileName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);

                if (startSubName > 0)
                {
                    fileName = fileName.Substring(startSubName);
                }
            }

            Debug.Log($"GameEvent \"{name}\" raised\n{formattedMethodName} (at {fileName}:{lineNumber})");
        }
#endif
    }
}
