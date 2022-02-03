using System.Diagnostics;
using System.Reflection;
using GraphProcessor;
using UnityEngine;

public abstract class PortUpdaterNode : BaseNode
{
    protected virtual void OnDataChanged(FieldInfo originField, UnityEditor.SerializedProperty serializedProperty)
    {
        UpdatePortsForFieldLocal(originField.Name);
    }
}
