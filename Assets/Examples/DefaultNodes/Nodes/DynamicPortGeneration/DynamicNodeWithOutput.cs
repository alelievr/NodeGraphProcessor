using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable]
public abstract class DynamicNodeWithOutput<T> : DynamicNode<T>
{
    [Output(name = "Out")]
    public T dataOutput;

    public override string name => "DynamicNodeWithOutput";

    protected override void Process()
    {
        base.Process();
        dataOutput = data;
    }
}
