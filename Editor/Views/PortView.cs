using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace GraphProcessor
{
	public class PortView : Port
	{
		public string				fieldName => fieldInfo.Name;
		public Type					fieldType => fieldInfo.FieldType;
		public new Type				portType;
        public BaseNodeView     	owner { get; private set; }
		public readonly PortData	portData;

		public event Action< PortView, Edge >	OnConnected;
		public event Action< PortView, Edge >	OnDisconnected;

		protected FieldInfo		fieldInfo;
		protected EdgeConnectorListener	listener;

		string userPortStyleFile = "PortViewTypes";

		List< EdgeView >		edges = new List< EdgeView >();

		public int connectionCount => edges.Count;

		readonly string portStyle = "GraphProcessorStyles/PortView";

        public PortView(Orientation orientation, Direction direction, FieldInfo fieldInfo, PortData portData, EdgeConnectorListener edgeConnectorListener)
            : base(orientation, direction, Capacity.Multi, portData.displayType ?? fieldInfo.FieldType)
		{
			styleSheets.Add(Resources.Load<StyleSheet>(portStyle));

			var userPortStyle = Resources.Load<StyleSheet>(userPortStyleFile);

			if (userPortStyle != null)
				styleSheets.Add(userPortStyle);

			this.m_EdgeConnector = new EdgeConnector< EdgeView >(edgeConnectorListener);
			this.AddManipulator(m_EdgeConnector);

			this.fieldInfo = fieldInfo;
			this.listener = edgeConnectorListener;
			this.portType = portData.displayType ?? fieldInfo.FieldType;
			this.portData = portData;
			this.portName = fieldName;
		}

		public virtual void Initialize(BaseNodeView nodeView, string name)
		{
			this.owner = nodeView;

			// Correct port type if port accept multiple values (and so is a container)
			if (portData.acceptMultipleEdges && portType == fieldType) // If the user haven't set a custom field type
			{
				Debug.Log("override display type: " + portType +  ", " + fieldType);
				portType = fieldType.GetGenericArguments()[0];
			}

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

			edges.Add(edge as EdgeView);

			inputNode.OnPortConnected(edge.input as PortView);
			outputNode.OnPortConnected(edge.output as PortView);
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