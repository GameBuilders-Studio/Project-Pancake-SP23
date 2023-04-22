using UnityEngine;
using UnityEngine.Events;

namespace GameBuilders.Events
{
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("Event to register with.")]
        public GameEvent Event;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent Response;

        [Tooltip("Whether to disable this event listener after the event is invoked.")]
        public bool TriggerOnce = false;

        private void OnEnable()
        {
            Event?.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event?.UnregisterListener(this);
        }

        public void OnEventRaised()
        {
            Response?.Invoke();

            if (TriggerOnce)
            {
                enabled = false;
            }
        }
    }
}