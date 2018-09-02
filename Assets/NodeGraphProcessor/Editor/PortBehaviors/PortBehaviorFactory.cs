using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
    public static class PortBehaviorFactory
    {
        static Dictionary<Type, Type> portBehaviors = new Dictionary<Type, Type>();

        static PortBehaviorFactory()
        {
			foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
			{
				var customPortBehaviorAttr = type.GetCustomAttribute< CustomPortBehaviorAttribute >();

				if (customPortBehaviorAttr == null)
					continue ;

				portBehaviors[customPortBehaviorAttr.targetType] = type;
			}
        }

        public static void CreatePortBehavior(BaseNodeView nodeView, FieldInfo field, Direction direction, EdgeConnectorListener listener, bool isMultiple, string name)
        {
            Type behaviorType;

            portBehaviors.TryGetValue(field.FieldType, out behaviorType);

            if (behaviorType == null)
                behaviorType = typeof(DefaultPortBehavior);
            
            Activator.CreateInstance(behaviorType, nodeView, field, direction, listener, isMultiple, name);
        }
    }
}