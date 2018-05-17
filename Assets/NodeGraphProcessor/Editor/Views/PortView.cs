using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using System;
using System.Reflection;

namespace GraphProcessor
{
	class PortView : Port
	{
		public bool				isMultiple;
		public int				index { get; private set; }
		FieldInfo				field;
		BaseNodeView			owner;
		EdgeConnectorListener	edgeConnectorListener;

		public PortView(Orientation portOrientation, Direction portDirection, FieldInfo field, EdgeConnectorListener edgeConnectorListener, BaseNodeView owner) : base(portOrientation, portDirection, field.FieldType)
		{
			AddStyleSheetPath("Styles/PortView");

			this.m_EdgeConnector = new EdgeConnector< EdgeView >(edgeConnectorListener);
			this.field = field;
			this.owner = owner;
			this.edgeConnectorListener = edgeConnectorListener;

			this.AddManipulator(m_EdgeConnector);

			portName = "Test";
			isMultiple = portType.IsGenericType && portType.GetGenericTypeDefinition() == typeof(PortArray<>);
			visualClass = "type";

			if (isMultiple)
				visualClass += field.FieldType.GetGenericArguments()[0].Name;
			else
				visualClass += field.FieldType.Name;

			Debug.Log("Created port with class: " + visualClass + ", from type: " + portType);
		}

		public override void Connect(Edge edge)
		{
			base.Connect(edge);

			if (direction == Direction.Output && isMultiple)
			{
				var portArray = field.GetValue(owner) as IList;
				var paramType = portArray.GetType().GetGenericArguments()[0];
				int index = portArray.Count;
				var newPort = new PortView(orientation, direction, field, edgeConnectorListener, owner);

				portArray.Add(Activator.CreateInstance(paramType));
				
				newPort.index = index;
				owner.AddPort(newPort);
			}
		}

		public override void Disconnect(Edge edge)
		{
			base.Disconnect(edge);

			if (direction == Direction.Input && isMultiple)
			{
				owner.RemovePort(this);
				//TODO: remove the element in the portArray
			}
		}
	}
}