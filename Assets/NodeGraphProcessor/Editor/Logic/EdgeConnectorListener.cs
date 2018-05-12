using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
    public sealed class EdgeConnectorListener : IEdgeConnectorListener
    {
        readonly BaseGraphView graphView;

        public EdgeConnectorListener(BaseGraphView graphView)
        {
            this.graphView = graphView;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
			//If the edge was already existing, remove it
			if (!edge.isGhostEdge)
			{
				graphView.Disconnect(edge as EdgeView);
			}

			//TODO: open new nodes selector and connect the created node if there is one
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
			var edgeView = edge as EdgeView;

			if (edgeView == null || edgeView.input == null || edgeView.output == null)
				return ;
			
			Debug.Log("ghost edge: " + edge.input.connected);
			Debug.Log("ghost edge: " + edge.output.connected);

			//If the edge was moved to another port
			if (edgeView.isConnected)
			{
				this.graphView.Disconnect(edgeView);
				Debug.Log("Edge disconnected !");
			}

			this.graphView.graph.RegisterCompleteObjectUndo("Connected " + edgeView.input.node.name + " and " + edgeView.output.node.name);
			this.graphView.Connect(edge as EdgeView);
        }
    }
}