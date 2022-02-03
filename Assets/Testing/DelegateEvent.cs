using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DelegateEvent<T> : ScriptableObject
{
    public delegate void EventHandler(object sender, VarObjectEventArgs args);
    public event EventHandler Event;

    public void AddListener(EventHandler listener)
    {
        Event += listener;
    }

    public void RemoveListener(EventHandler listener)
    {
        Event -= listener;
    }

    public void Raise(T argument)
    {
        if (Event != null)
            Event.Invoke(this, new VarObjectEventArgs(argument));
    }
}
