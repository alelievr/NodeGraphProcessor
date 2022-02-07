using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[Serializable]
public class Namer
{
    [SerializeField, Input("Name"), ShowAsDrawer] string name;
    [SerializeField, Input("Bool")] bool value;
}