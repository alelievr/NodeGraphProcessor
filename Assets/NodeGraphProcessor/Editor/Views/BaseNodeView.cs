using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Rendering;
using UnityEditor;
using System.Reflection;

using NodeView = UnityEditor.Experimental.UIElements.GraphView.Node;

namespace GraphProcessor
{
	[NodeCustomEditor(typeof(BaseNode))]
	public class BaseNodeView : NodeView
	{
		public BaseNode				nodeTarget;

		public List< Port >			inputPorts = new List< Port >();
		public List< Port >			outputPorts = new List< Port >();

		Dictionary< string, Port >	portsPerFieldName = new Dictionary< string, Port >();

		protected BaseGraphView		owner;

        protected VisualElement 	controlsContainer;

		public void Initialize(BaseGraphView owner, BaseNode node)
		{
			nodeTarget = node;
			this.owner = owner;
			
			AddStyleSheetPath("Styles/BaseNodeView");
			
            controlsContainer = new VisualElement{ name = "controls" };
        	mainContainer.Add(controlsContainer);

			InitializePorts();

			InitializeView();

			Enable();
		}

		public void AddPort(Port p)
		{
			if (p.direction == Direction.Input)
			{
				inputPorts.Add(p);
				inputContainer.Add(p);
			}
			else
			{
				outputPorts.Add(p);
				outputContainer.Add(p);
			}

			portsPerFieldName[p.portName] = p;
		}

		public void RemovePort(Port p)
		{
			if (p.direction == Direction.Input)
			{
				inputPorts.Remove(p);
				inputContainer.Remove(p);
			}
			else
			{
				outputPorts.Remove(p);
				outputContainer.Remove(p);
			}

			portsPerFieldName.Remove(p.portName);
		}

		void InitializePorts()
		{
			var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var field in fields)
			{
				var inputAttribute = field.GetCustomAttribute< InputAttribute >();
				var outputAttribute = field.GetCustomAttribute< OutputAttribute >();

				if (inputAttribute == null && outputAttribute == null)
					continue ;

				Debug.Log("adding port: " + field);
				
				PortView port = new PortView(
					Orientation.Horizontal,
					(inputAttribute != null) ? Direction.Input : Direction.Output,
					field,
					owner.connectorListener,
					this
				);

				port.portName = field.Name;

				AddPort(port);
			}
		}

		void InitializeView()
		{
			title = nodeTarget.name;

			SetPosition(nodeTarget.position);
		}

		public Port GetPortFromFieldName(string fieldName)
		{
			Port	ret;

			portsPerFieldName.TryGetValue(fieldName, out ret);

			return ret;
		}

		public virtual void Enable()
		{
			DrawDefaultInspector();
			//TODO: draw custom inspector with reflection
		}

		public virtual void DrawDefaultInspector()
		{
		}

		public override void SetPosition(Rect newPosition)
		{
			base.SetPosition(newPosition);

			nodeTarget.position = newPosition;
		}

		public override bool	expanded
		{
			get { return base.expanded; }
			set
			{
				base.expanded = value;
				nodeTarget.expanded = value;
			}
		}
	}
}