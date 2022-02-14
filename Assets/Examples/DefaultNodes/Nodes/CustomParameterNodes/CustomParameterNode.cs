using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/CustomParameterNode")]
public class CustomParameterNode : ParameterNode
{
    protected override IEnumerable<PortData> GetOutputPort(List<SerializableEdge> edges)
    {
        return new List<PortData>();
    }
}
