using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

[System.Serializable]
public class MyFloatParam : FloatParameter
{
    public override Type CustomParameterNodeType => typeof(CustomParameterNode);
}
