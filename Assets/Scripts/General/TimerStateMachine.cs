using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerStateMachine<T> where T : Enum //NOTE: Does not require base state, as it accounts for that when time = 0;
{
    public float TotalTime { get; private set; }
    public T DefaultState { get; private set; }

    public HashSet<TimerMarker<T>> timers;

    public T state;

    private float timerTime = 0.0f;

    private bool isPaused = false;

    public void TogglePause() => isPaused = !isPaused;

    public TimerStateMachine(T defaultStateIn, IEnumerable<TimerMarker<T>> timersIn)
    {
        timers = new();
        foreach (var timer in timersIn)
        {
            timers.Add(timer);
        }
        DefaultState = defaultStateIn;
    }

    //Removes all timer markers except the default timer marker, uses LINQ for predicate
    public void RemoveExceptDefault(bool autoReset = true)
    {
        timers.RemoveWhere(marker => !marker.State.Equals(DefaultState));
        if (autoReset)
        {
            Reset();
        }
    }

    public void Reset()
    {
        timerTime = TotalTime;
        state = DefaultState;
    }

    public void SetTotalTime(float baseTime, bool autoReset = true)
    {
        TotalTime = 0.0f;
        foreach (TimerMarker<T> timer in timers)
        {
            TotalTime += timer.Time;
        }

        Debug.Log("Timermarker Size: " + timers.Count);

        TotalTime += baseTime;

        if (autoReset)
        {
            Reset();
        }
    }

    public void Decrement(float deltaTime)
    {
        if (isPaused) //Default case for being paused
        {
            return;
        }
        if (timerTime <= 0.0f) //When timer is less than zero
        {
            state = DefaultState;
            return;
        }
        timerTime = Mathf.Max(timerTime - deltaTime, 0.0f); //Clamps the minimum so attack cooldown does not become negatives

        foreach (TimerMarker<T> timer in timers) //Iterates through all time markers
        {
            if (!timer.State.Equals(state)) //Checking against duplicate state and checking if the new time is not equal to 0
            {
                Debug.Log("Current Time: " + timerTime);
                state = timer.State;

                foreach (Action action in timer.Actions) //Performs all assigned actions
                {
                    action();
                }
                return; //Terminates early
            }
        }
    }
}

public class TimerMarker<T> where T : Enum
{
    public TimerMarker(float timeIn, T stateIn, params Action[] actionsIn)
    {
        Actions = new();

        Time = timeIn;
        State = stateIn;
        foreach (Action action in actionsIn)
        {
            Actions.Add(action);
        }
    }

    public float Time { get; set; }
    public T State { get; set; }
    public HashSet<Action> Actions { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is null || obj is not TimerMarker<T>)
        {
            return false;
        }

        TimerMarker<T> compare = obj as TimerMarker<T>;
        return compare.State.Equals(State) && compare.Time == Time; //Cannot use == on generics
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Time, State);
    }
}