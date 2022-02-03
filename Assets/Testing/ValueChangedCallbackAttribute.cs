using System;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Check if a given field has changed and if it has calls a method.
/// </summary>
/// 
public class ValueChangedCallbackAttribute : PropertyAttribute
{
    string fieldName;
    public string FieldName => fieldName;

    string methodName;
    public string MethodName => methodName;

    /// <summary>
    /// </summary>
    public ValueChangedCallbackAttribute(string fieldName, string methodName)
    {
        this.fieldName = fieldName;
        this.methodName = methodName;
    }
}