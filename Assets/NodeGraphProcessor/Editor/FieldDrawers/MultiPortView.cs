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
    [CustomPortView(typeof(MultiPorts))]
    public class MultiPortView : PortView
    {
        MultiPorts          multiPorts;
        Dictionary< Edge, PortView >    portViews = new Dictionary< Edge, PortView >();
        BaseNodeView        owner;
        
		public int				    id;

        public MultiPortView(Orientation portOrientation, Direction direction, FieldInfo fieldInfo, EdgeConnectorListener edgeConnectorListener)
            : base(portOrientation, direction, fieldInfo, edgeConnectorListener) {}

        public override void Initialize(BaseNodeView nodeView, bool isMultiple, string name)
        {
            base.Initialize(nodeView, isMultiple, name);

            owner = nodeView;
            multiPorts = fieldInfo.GetValue(nodeView.nodeTarget) as MultiPorts;

            // Initialize the MultiPort field if null
            if (multiPorts == null)
            {
                multiPorts = new MultiPorts();
                fieldInfo.SetValue(nodeView.nodeTarget, multiPorts);
            }
            
            // Get the last identifier on the list (pushed before this port was created either by this.Connect or the MultiPort constructor)
            id = multiPorts.portIds.Last();

            // if there is no controller yet, set our instance to be the controller
            if (multiPorts.viewController == null)
                multiPorts.viewController = this;
            else
                return ;
            
            // Instantiate all ports needed to create the serialized connections
            for (int i = 0; i < multiPorts.portCount; i++)
                AddPort();
        }

        PortView AddPort()
        {
            PortView pv = owner.AddPort(fieldInfo, direction, listener, isMultiple, name);

            // We force the AddPort in the BaseNode class because the port list is not updated except at the construction of the class
            owner.nodeTarget.AddPort(direction == Direction.Input, fieldName);

            return pv;
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);

            // If another MultiPortView is controlling this MultiPort, we have noting to do
            if (multiPorts.viewController != this)
                return ;
            
            multiPorts.AddUniqueId(multiPorts.GetUniqueId());
            portViews[edge] = AddPort();
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            
            // If another MultiPortView is controlling this MultiPort, we have noting to do
            if (multiPorts.viewController != this)
                return ;
            
            if ((edge as EdgeView).isConnected)
                owner.RemovePort(portViews[edge]);
        }
    }
}