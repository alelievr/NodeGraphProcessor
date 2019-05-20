using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    public sealed class EdgeConnectorListener : IEdgeConnectorListener
    {
        readonly BaseGraphView graphView;

        Dictionary< Edge, PortView >    edgeInputPorts = new Dictionary< Edge, PortView >();
        Dictionary< Edge, PortView >    edgeOutputPorts = new Dictionary< Edge, PortView >();

        public EdgeConnectorListener(BaseGraphView graphView)
        {
            this.graphView = graphView;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
			this.graphView.RegisterCompleteObjectUndo("Disconnect edge");

			//If the edge was already existing, remove it
			if (!edge.isGhostEdge)
				graphView.Disconnect(edge as EdgeView);

			//TODO: open new nodes selector and connect the created node if there is one
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
			var edgeView = edge as EdgeView;
            bool wasOnTheSamePort = false;

			if (edgeView?.input == null || edgeView?.output == null)
				return ;

			//If the edge was moved to another port
			if (edgeView.isConnected)
			{
                if (edgeInputPorts.ContainsKey(edge) && edgeOutputPorts.ContainsKey(edge))
                    if (edgeInputPorts[edge] == edge.input && edgeOutputPorts[edge] == edge.output)
                        wasOnTheSamePort = true;

                if (!wasOnTheSamePort)
                    this.graphView.Disconnect(edgeView);
			}

            if (edgeView.input.node == null || edgeView.output.node == null)
                return;

            edgeInputPorts[edge] = edge.input as PortView;
            edgeOutputPorts[edge] = edge.output as PortView;
			this.graphView.RegisterCompleteObjectUndo("Connected " + edgeView.input.node.name + " and " + edgeView.output.node.name);
			if (!this.graphView.Connect(edge as EdgeView, autoDisconnectInputs: !wasOnTheSamePort))
            {
                this.graphView.Disconnect(edge as EdgeView);
            }
        }
    }
}