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
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    public static void AddEvent(string eventName, UnityAction listener)
    {
        if (Instance == null)
        {
            Debug.LogWarning("EventManager does not init");
            return;
        }
        // Debug.Log(Instance);
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            Instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    /// <summary>
    /// Remove event to the event dictionary
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>

    public static void RemoveEvent(string eventName, UnityAction listener)
    {
        if (Instance == null)
        {
            Debug.LogWarning("EventManager does not init");
            return;
        }
        if (!Instance.enabled)
        {
            Debug.LogWarning("EventManager disabled");
            return;
        }
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    /// <summary>
    /// Remove All Events
    /// </summary>
    /// <param name="eventName"></param>
    public static void RemoveAllEvent(string eventName)
    {
        if (Instance == null)
        {
            //Debug.LogWarning("EventManager does not init");
            return;
        }
        if (!Instance.enabled)
        {
            Debug.LogWarning("EventManager disabled");
            return;
        }
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
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
