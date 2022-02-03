using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class MyInputAttribute : InputAttribute
{
    public readonly Type InputType;
    public readonly InputSortType SortType;

    public MyInputAttribute(string name = null, bool allowMultiple = false, InputSortType sortType = InputSortType.FIRST_IN, Type inputType = null)
    {
        this.name = name;
        this.allowMultiple = allowMultiple;
        this.SortType = sortType;
        this.InputType = inputType;
    }
}

public enum InputSortType { FIRST_IN, POSITION_Y }
