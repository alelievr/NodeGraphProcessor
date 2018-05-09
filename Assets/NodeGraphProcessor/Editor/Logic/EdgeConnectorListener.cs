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
			//TODO: open new nodes selector and connect the created node if there is one
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            var leftSlot = edge.output;
            var rightSlot = edge.input;
            if (leftSlot != null && rightSlot != null)
            {
				this.graphView.graph.RegisterCompleteObjectUndo("Nothing ?");
				//TODO: Register undo
				//TODO: tell to the graph to connect these slots
                // m_Graph.owner.RegisterCompleteObjectUndo("Connect Edge");
                // m_Graph.Connect(leftSlot.slotReference, rightSlot.slotReference);
            }
        }
    }
}