using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System.Reflection;
using System;

[System.Serializable]
public abstract class DynamicNode<T> : PortUpdaterNode
{
    [Input("Action Data", true)]
    public Dictionary<string, List<object>> actionData = new Dictionary<string, List<object>>();

    [ExpandableSO, ValueChangedCallback(nameof(actionData), nameof(OnDataChanged))]
    public T data;

    public override bool needsInspector => true;

    protected override void Process()
    {
        UpdateActionWithCustomPortData();
    }

    protected virtual void UpdateActionWithCustomPortData()
    {
        // We clone due to reference issues 
        Dictionary<string, List<object>> actionDataClone = new Dictionary<string, List<object>>(actionData);

        foreach (var field in GetInputFieldsOfType())
        {
            if (!actionDataClone.ContainsKey(field.fieldInfo.Name))
            {
                if (field.inputAttribute.showAsDrawer)
                    continue;

                field.fieldInfo.SetValue(data, default);
                continue;
            }

            if (field.inputAttribute is MyInputAttribute)
            {
                MyInputAttribute inputAttribute = field.inputAttribute as MyInputAttribute;
                if (inputAttribute.InputType != null && field.fieldInfo.FieldType.GetInterfaces().Any(x => x == typeof(IList)))
                {
                    IList list = Activator.CreateInstance(field.fieldInfo.FieldType) as IList;
                    foreach (var value in actionDataClone[field.fieldInfo.Name])
                        list.Add(value);

                    field.fieldInfo.SetValue(data, list);
                    continue;
                }
            }

            field.fieldInfo.SetValue(data, actionDataClone[field.fieldInfo.Name][0]);
        }

        actionData.Clear();
    }

    #region Reflection Generation Of Ports

    private List<FieldPortInfo> GetInputFieldsOfType()
    {
        List<FieldPortInfo> foundInputFields = new List<FieldPortInfo>();

        Type dataType = data != null ? data.GetType() : typeof(T);
        foreach (var field in dataType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            foreach (var attribute in field.GetCustomAttributes(typeof(InputAttribute), true))
            {
                if (attribute.GetType() != typeof(InputAttribute) && !attribute.GetType().IsSubclassOf(typeof(InputAttribute))) continue;

                foundInputFields.Add(new FieldPortInfo(field, attribute as InputAttribute));
                break;
            }
        }

        return foundInputFields;
    }

    private FieldPortInfo GetFieldPortInfo(string fieldName)
    {
        Type dataType = data != null ? data.GetType() : typeof(T);

        FieldInfo fieldInfo = dataType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        InputAttribute inputAttribute = fieldInfo.GetCustomAttribute<InputAttribute>();

        return new FieldPortInfo(fieldInfo, inputAttribute);
    }

    [CustomPortInput(nameof(actionData), typeof(object))]
    protected void PullInputs(List<SerializableEdge> connectedEdges)
    {
        if (connectedEdges.Count == 0) return;

        FieldPortInfo field = GetFieldPortInfo(connectedEdges.ElementAt(0).inputPortIdentifier);

        if (actionData == null) actionData = new Dictionary<string, List<object>>();
        foreach (var edge in connectedEdges.GetNonRelayEdges().OrderByInputAttribute(field.inputAttribute))
        {
            if (!actionData.ContainsKey(field.fieldInfo.Name))
                actionData.Add(field.fieldInfo.Name, new List<object>());

            actionData[field.fieldInfo.Name].Add(edge.passThroughBuffer);
        }
    }

    [CustomPortBehavior(nameof(actionData))]
    protected IEnumerable<PortData> ActionDataBehaviour(List<SerializableEdge> edges) // Try changing edge here when ports update  
    {
        foreach (var field in GetInputFieldsOfType())
        {
            Type displayType = field.fieldInfo.FieldType;

            if (field.inputAttribute is MyInputAttribute)
            {
                MyInputAttribute inputAttribute = field.inputAttribute as MyInputAttribute;
                if (inputAttribute.InputType != null)
                    displayType = inputAttribute.InputType;
            }

            yield return new PortData
            {
                displayName = field.inputAttribute.name,
                displayType = displayType,
                identifier = field.fieldInfo.Name,
                showAsDrawer = field.inputAttribute.showAsDrawer,
                vertical = false,
                proxiedFieldPath = "data." + field.fieldInfo.Name,
                acceptMultipleEdges = field.inputAttribute.allowMultiple,
            };
        }

        // Debug.Log(this.GetCustomName() + " BEHAVE: " + this.inputPorts.Count); 
    }

    // public override IEnumerable<FieldInfo> OverrideFieldOrder(IEnumerable<FieldInfo> fields)   
    // {
    //     return base.OverrideFieldOrder(fields).Reverse();

    //     // static long GetFieldInheritanceLevel(FieldInfo f)
    //     // {
    //     //     int level = 0;
    //     //     var t = f.DeclaringType;
    //     //     while (t != null)
    //     //     {
    //     //         t = t.BaseType;
    //     //         level++;
    //     //     }

    //     //     return level;
    //     // }

    //     // // Order by MetadataToken and inheritance level to sync the order with the port order (make sure FieldDrawers are next to the correct port)
    //     // return fields.OrderByDescending(f => (GetFieldInheritanceLevel(f) << 32) | (long)f.MetadataToken);

    // }

    #endregion
}

public struct FieldPortInfo
{
    public FieldInfo fieldInfo;
    public InputAttribute inputAttribute;

    public FieldPortInfo(FieldInfo fieldInfo, InputAttribute inputAttribute)
    {
        this.fieldInfo = fieldInfo;
        this.inputAttribute = inputAttribute;
    }
}