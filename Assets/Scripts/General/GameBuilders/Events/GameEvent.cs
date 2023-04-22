using System.Collections.Generic;
using UnityEngine;

namespace GameBuilders.Events
{
    [CreateAssetMenu]
    public class GameEvent : ScriptableObject
    {
        [SerializeField]
        [TextArea(3, 10)]
        private string _description;

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
    }
}
