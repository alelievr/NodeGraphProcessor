using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using static SequenceName;

[System.Serializable, NodeMenuItem("Custom/ConditionalNameNode")]
public class ConditionalNameNode : DynamicNodeWithOutput<ConditionalName>
{
    public override string name => "ConditionalNameNode";
}
