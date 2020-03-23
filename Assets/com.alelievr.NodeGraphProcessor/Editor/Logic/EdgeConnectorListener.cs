using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    /// <summary>
    /// Base class to write your own edge handling connection system
    /// </summary>
    public class BaseEdgeConnectorListener : IEdgeConnectorListener
    {
        readonly BaseGraphView graphView;

        Dictionary< Edge, PortView >    edgeInputPorts = new Dictionary< Edge, PortView >();
        Dictionary< Edge, PortView >    edgeOutputPorts = new Dictionary< Edge, PortView >();

        public BaseEdgeConnectorListener(BaseGraphView graphView)
        {
            this.graphView = graphView;
        }

        public virtual void OnDropOutsidePort(Edge edge, Vector2 position)
        {
			this.graphView.RegisterCompleteObjectUndo("Disconnect edge");

			//If the edge was already existing, remove it
			if (!edge.isGhostEdge)
				graphView.Disconnect(edge as EdgeView);

            // when on of the port is null, then the edge was created and dropped outside of a port
            if (edge.input == null || edge.output == null)
            {
                Vector2 mousePos = graphView.ChangeCoordinatesTo(graphView.contentViewContainer, position);

                // Empirical offset:
                mousePos += new Vector2(-10f, -18);

                // TODO: function
                this.graphView.RegisterCompleteObjectUndo("Added relay node ");
                var relayNode = BaseNode.CreateFromType<RelayNode>(mousePos);
                var relayView = graphView.AddNode(relayNode);

                // Connect to the new relay node:
                var port = edge.output ?? edge.input;
                var pv = port as PortView;
                bool isInput = edge.input == null;

                var p = pv.owner.nodeTarget.GetPort(pv.fieldName, pv.portData.identifier);

                // Wait for node to be created ...
                graphView.schedule.Execute(() => {
                    // We can't use edge here because it have been destroyed
                    if (isInput)
                    {
                        var newEdge = graphView.graph.Connect(relayNode.inputPorts[0], p);
                        graphView.ConnectView(new EdgeView() {
                            userData = newEdge,
                            input = relayView.GetPortViewFromFieldName(newEdge.inputFieldName, newEdge.inputPortIdentifier),
                            output = pv.owner.GetPortViewFromFieldName(newEdge.outputFieldName, newEdge.outputPortIdentifier)
                        });
                    }
                    else
                    {
                        var newEdge = graphView.graph.Connect(p, relayNode.outputPorts[0]);
                        graphView.ConnectView(new EdgeView() {
                            userData = newEdge,
                            input = pv.owner.GetPortViewFromFieldName(newEdge.inputFieldName, newEdge.inputPortIdentifier),
                            output = relayView.GetPortViewFromFieldName(newEdge.outputFieldName, newEdge.outputPortIdentifier)
                        });
                    }
                }).ExecuteLater(1);

            }

			//TODO: open new nodes selector and connect the created node if there is one
        }

        public virtual void OnDrop(GraphView graphView, Edge edge)
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