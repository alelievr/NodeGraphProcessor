<<<<<<< HEAD
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
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
=======
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
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
>>>>>>> 85ff4ace9e4636013762a222efd3312ae30ff3ce
}