using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class EventDictionary : SerializableDictionary<string, UnityEvent>
{}


public class EventManager : Singleton<EventManager>
{
    [SerializeField]
    [Tooltip("key is the event name, value is the listners that will be invoked when the event is invoked")]
    private EventDictionary eventDictionary;
    
    private void OnDisable()
    {
        foreach (var item in eventDictionary)
        {
            if (item.Value == null) return;
            item.Value.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Register event to the event diciotnary
    /// </summary>
    /// <param name="listenerName"></param>
    /// <param name="listener"></param>
    public static void AddListener(string listenerName, UnityAction listener)
    {
        if (Instance == null)
        {
            Debug.LogError("EventManager does not init");
            return;
        }
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(listenerName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            Instance.eventDictionary.Add(listenerName, thisEvent);
        }
    }

    /// <summary>
    /// Remove event to the event dictionary
    /// </summary>
    /// <param name="listenerName"></param>
    /// <param name="listener"></param>

    public static void RemoveListener(string listenerName, UnityAction listener)
    {
        if (Instance == null)
        {
            Debug.LogError("EventManager does not init");
            return;
        }
        if (!Instance.enabled)
        {
            Debug.LogError("EventManager disabled");
            return;
        }
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(listenerName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    /// <summary>
    /// Remove All Events
    /// </summary>
    /// <param name="listenerName"></param>
    public static void RemoveAllListener(string listenerName)
    {
        if (Instance == null)
        {
            Debug.LogError("EventManager does not init");
            return;
        }
        if (!Instance.enabled)
        {
            Debug.LogError("EventManager disabled");
            return;
        }
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(listenerName, out thisEvent))
        {
            thisEvent.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Invoke event in the eventDictionary, will not be invoke if the eventName does not exist.
    /// </summary>
    /// <param name="eventName"></param>
    public static void Invoke(string eventName)
    {
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary == null)
        {
            Instance.eventDictionary = new EventDictionary();
        }
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
        else
        {
            Debug.LogWarning($"The event: {eventName} does not exist in the EventManager");
        }
    }


}
