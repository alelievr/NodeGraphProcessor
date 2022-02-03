using System.Data.SqlTypes;
using UnityEngine;

/// <summary>
/// 
/// CallbackObject callback is used as a OnValueChanged event.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SharedVariable<T> : DelegateEvent<T>
{
    [SerializeField] protected T InitialValue;

    protected T runtimeValue;
    public virtual T RuntimeValue
    {
        get => runtimeValue;
        set
        {
            if (runtimeValue == null && value == null) return;
            else if (runtimeValue != null && runtimeValue.Equals(value)) return;
            else if (value != null && value.Equals(runtimeValue)) return;

            runtimeValue = value;
            Raise(value);
        }
    }

    // Initialize runtime value with editor's value
    protected virtual void OnEnable()
    {
        RuntimeValue = InitialValue;
    }
}