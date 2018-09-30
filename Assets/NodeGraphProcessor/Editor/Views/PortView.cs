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
		public string			fieldName { get; protected set; }
		public new Type			portType;
        public BaseNodeView     owner { get; private set; }

		public event Action< PortView, Edge >	OnConnected;
		public event Action< PortView, Edge >	OnDisconnected;

		protected FieldInfo		fieldInfo;
		protected EdgeConnectorListener	listener;

		string userPortStyleFile = "PortViewTypes";

		List< EdgeView >		edges = new List< EdgeView >();

        public PortView(Orientation portOrientation, Direction direction, FieldInfo fieldInfo, EdgeConnectorListener edgeConnectorListener)
            : base(portOrientation, direction, Capacity.Multi, fieldInfo.FieldType)
		{
			AddStyleSheetPath("GraphProcessorStyles/PortView");

			if (Resources.Load< UnityEngine.Object >(userPortStyleFile) != null)
				AddStyleSheetPath(userPortStyleFile);

			this.m_EdgeConnector = new EdgeConnector< EdgeView >(edgeConnectorListener);
			this.AddManipulator(m_EdgeConnector);

			fieldName = fieldInfo.Name;
			portType = fieldInfo.FieldType;

			this.fieldInfo = fieldInfo;
			this.listener = edgeConnectorListener;
		}

		public virtual void Initialize(BaseNodeView nodeView, bool isMultiple, string name)
		{
			this.isMultiple = isMultiple;
			this.owner = nodeView;

			// Correct port type if port accept multiple values (and so is a container)
			if (isMultiple)
				portType = portType.GetGenericArguments()[0];
				
			if (name != null)
				portName = name;
			visualClass = "Port_" + portType.Name;
		}

		public override void Connect(Edge edge)
		{
			OnConnected?.Invoke(this, edge);
			
			base.Connect(edge);

			var inputNode = (edge.input as PortView).owner;
			var outputNode = (edge.output as PortView).owner;

			inputNode.OnPortConnected(edge.input as PortView);
			outputNode.OnPortConnected(edge.output as PortView);

			edges.Add(edge as EdgeView);
		}

		public override void Disconnect(Edge edge)
		{
			OnDisconnected?.Invoke(this, edge);

			base.Disconnect(edge);

			if (!(edge as EdgeView).isConnected)
				return ;

			var inputNode = (edge.input as PortView).owner;
			var outputNode = (edge.output as PortView).owner;

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