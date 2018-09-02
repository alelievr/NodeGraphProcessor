using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace GraphProcessor
{
    [CustomPortBehavior(typeof(MultiPorts))]
    public class MultiPortBehavior
    {
        MultiPorts                      multiPorts;
        BaseNodeView                    node;
        Dictionary< Edge, PortView >    portViews = new Dictionary< Edge, PortView >();

        FieldInfo                       fieldInfo;
        Direction                       direction;
        EdgeConnectorListener           listener;
        bool                            isMultiple;
        string                          name;

        public MultiPortBehavior(BaseNodeView nodeView, FieldInfo fieldInfo, Direction direction, EdgeConnectorListener listener, bool isMultiple, string name)
        {
            this.multiPorts = fieldInfo.GetValue(nodeView.nodeTarget) as MultiPorts;
            this.node = nodeView;
            this.fieldInfo = fieldInfo;
            this.direction = direction;
            this.listener = listener;
            this.isMultiple = isMultiple;
            this.name = name;

            // Initialize the MultiPort field if null
            if (multiPorts == null)
            {
                multiPorts = new MultiPorts();
                fieldInfo.SetValue(nodeView.nodeTarget, multiPorts);
            }
            
            // Instantiate all ports needed to create the serialized connections
            // Minus one because we count our current instance
            for (int i = 0; i < multiPorts.portCount; i++)
                AddPort();
        }

        void AddPort()
        {
            PortView pv = node.AddPort(fieldInfo, direction, listener, isMultiple, name);

            // We force the AddPort in the BaseNode class because the port list is not updated except at the construction of the class
            node.nodeTarget.AddPort(direction == Direction.Input, pv.fieldName);

            pv.OnConnected += OnPortConnected;
            pv.OnDisconnected += OnPortDisconnected;
        }

        public void OnPortConnected(PortView pv, Edge edge)
        {
            // Fix port datas
            if (pv.direction == Direction.Input)
                edge.input = pv;
            else
                edge.output = pv;

            // If the edge is already connected, ignore it
            if (portViews.ContainsKey(edge))
                return ;
            
            portViews[edge] = pv;

            if (pv.GetEdges().Count == 0)
            {
                multiPorts.AddUniqueId(multiPorts.GetUniqueId());
                AddPort();
            }
        }

        public void OnPortDisconnected(PortView pv, Edge edge)
        {
            if (pv.GetEdges().Count == 0)
            {
                if ((edge as EdgeView).isConnected && portViews.ContainsKey(edge))
                {
                    var portToRemove = portViews[edge];

                    node.RemovePort(portToRemove);
                    
                    node.nodeTarget.RemovePort(direction == Direction.Input, portToRemove.fieldName);
                    
                    portViews.Remove(edge);
                    multiPorts.RemoveUniqueId(0);
                }
            }
        }
    }
}