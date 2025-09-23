using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/ProxiedInputsNode")]
public class NamerNode : DynamicNodeWithOutput<Namer>
{
    public override string name => "ConditionalNameNode";
}
