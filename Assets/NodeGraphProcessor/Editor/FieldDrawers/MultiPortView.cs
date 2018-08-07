using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine;
using System.Reflection;

namespace GraphProcessor
{
    [CustomPortView(typeof(MultiPorts))]
    public class MultiPortView : PortView
    {
        public MultiPortView(Orientation portOrientation, Direction direction, FieldInfo fieldInfo, EdgeConnectorListener edgeConnectorListener)
            : base(portOrientation, direction, fieldInfo, edgeConnectorListener) {}

        public override void Initialize(BaseNodeView nodeView, bool isMultiple, string name)
        {
            base.Initialize(nodeView, isMultiple, name);
            //TODO
        }
    }
}