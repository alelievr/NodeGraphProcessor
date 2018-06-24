using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using System;
using System.Reflection;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace GraphProcessor
{
	public class PortView : Port
	{
		public bool				isMultiple;
		public string			fieldName { get; private set; }
		public new Type			portType;

		string userPortStyleFile = "PortViewTypes";

		List< EdgeView >		edges = new List< EdgeView >();

        public PortView(Orientation portOrientation, BaseNode.NodeFieldInformation field, EdgeConnectorListener edgeConnectorListener)
            : base(portOrientation, field.input ? Direction.Input : Direction.Output, Capacity.Multi, field.info.FieldType)
		{
			AddStyleSheetPath("GraphProcessorStyles/PortView");

			if (Resources.Load< UnityEngine.Object >(userPortStyleFile) != null)
				AddStyleSheetPath(userPortStyleFile);

			this.m_EdgeConnector = new EdgeConnector< EdgeView >(edgeConnectorListener);

			this.AddManipulator(m_EdgeConnector);

			portName = field.name;
			fieldName = field.info.Name;
			isMultiple = field.isMultiple;
			portType = field.info.FieldType;

			// Correct port type if port accept multiple values (and so is a container)
			if (field.isMultiple)
				portType = portType.GetGenericArguments()[0];

			visualClass = "Port_" + portType.Name;
		}

		public override void Connect(Edge edge)
		{
			base.Connect(edge);

			var inputNode = edge.input.node as BaseNodeView;
			var outputNode = edge.output.node as BaseNodeView;

			inputNode.OnPortConnected(edge.input as PortView);
			outputNode.OnPortConnected(edge.output as PortView);

			edges.Add(edge as EdgeView);
		}

		public override void Disconnect(Edge edge)
		{
			base.Disconnect(edge);

			if (!(edge as EdgeView).isConnected)
				return ;

			var inputNode = edge.input.node as BaseNodeView;
			var outputNode = edge.output.node as BaseNodeView;

			inputNode.OnPortDisconnected(edge.input as PortView);
			outputNode.OnPortDisconnected(edge.output as PortView);
			
			edges.Remove(edge as EdgeView);
		}

		public List< EdgeView >	GetEdges()
		{
			return edges;
		}
	}
}