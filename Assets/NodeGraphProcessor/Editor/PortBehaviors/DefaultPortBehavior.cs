using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace GraphProcessor
{
    public class DefaultPortBehavior
    {
        public DefaultPortBehavior(BaseNodeView nodeView, FieldInfo fieldInfo, Direction direction, EdgeConnectorListener listener, bool isMultiple, string name)
        {
            nodeView.AddPort(fieldInfo, direction, listener, isMultiple, name);
        }
    }
}