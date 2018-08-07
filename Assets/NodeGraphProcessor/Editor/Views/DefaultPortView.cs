using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine;
using System.Reflection;

namespace GraphProcessor
{
	[CustomPortView(priority: -100)]
    public class DefaultPortView : PortView
    {
        public DefaultPortView(Orientation portOrientation, Direction direction, FieldInfo fieldInfo, EdgeConnectorListener edgeConnectorListener)
            : base(portOrientation, direction, fieldInfo, edgeConnectorListener) {}
    }
}