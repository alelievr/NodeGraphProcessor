using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using System;
using System.Reflection;

namespace GraphProcessor
{
	public class PortView : Port
	{
		public bool				isMultiple;
		public string			fieldName { get { return field.Name; } }
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

			portName = field.Name;
			isMultiple = portType.IsGenericType && portType.GetGenericTypeDefinition() == typeof(PortArray<>);
			visualClass = "type";

			if (isMultiple)
				visualClass += field.FieldType.GetGenericArguments()[0].Name;
			else
				visualClass += field.FieldType.Name;
		}

		public override void Connect(Edge edge)
		{
			base.Connect(edge);

			if (direction == Direction.Input && isMultiple)
			{
				var portArray = field.GetValue(owner.nodeTarget) as IList;

				//Initialize the array if not
				if (portArray == null)
				{
					portArray = Activator.CreateInstance(field.FieldType) as IList;
					field.SetValue(owner.nodeTarget, portArray);
				}

				var paramType = portArray.GetType().GetGenericArguments()[0];
				int index = portArray.Count;
				var newPort = new PortView(orientation, direction, field, edgeConnectorListener, owner);

				portArray.Add(Activator.CreateInstance(paramType));

				newPort.portName = portName;
				
				owner.AddPort(newPort);
			}
		}

		public override void Disconnect(Edge edge)
		{
			var edgeView = edge as EdgeView;

			base.Disconnect(edge);

			if (!edgeView.isConnected)
				return ;

			if (direction == Direction.Input && isMultiple)
			{
				owner.RemovePort(this);
				//TODO: remove the element in the portArray
			}
		}
	}
}