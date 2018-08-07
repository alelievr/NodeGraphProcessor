using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Reflection;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
    public static class PortViewFactory
    {
        static List< CustomPortView > portViews = new List< CustomPortView >();

        class CustomPortView
        {
            public Type targetType;
            public int  priority;
            public Type portViewType;
        }

        static PortViewFactory()
        {
			foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
			{
				var customPortViewAttr = type.GetCustomAttribute< CustomPortViewAttribute >();

				if (customPortViewAttr == null)
					continue ;

				portViews.Add(new CustomPortView {
                    targetType = customPortViewAttr.targetType,
                    priority = customPortViewAttr.priority,
                    portViewType = type,
                });
			}

            portViews = portViews.OrderByDescending(k => k.priority).ToList();
        }

        public static PortView Create(FieldInfo fieldInfo, Direction direction, EdgeConnectorListener listener)
        {
            //TODO: hardcoded values
            object orientation = Orientation.Horizontal;

            foreach (var portView in portViews)
            {
                if (portView.targetType.IsAssignableFrom(fieldInfo.FieldType))
                    return Activator.CreateInstance(portView.portViewType, orientation, direction, fieldInfo, listener) as PortView;
            }

            return null;
        }
    }
}