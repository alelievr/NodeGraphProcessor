using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

using StatusFlags = UnityEngine.Experimental.UIElements.ContextualMenu.MenuAction.StatusFlags;
using NodeView = UnityEditor.Experimental.UIElements.GraphView.Node;

namespace GraphProcessor
{
	[NodeCustomEditor(typeof(BaseNode))]
	public class BaseNodeView : NodeView
	{
		public BaseNode							nodeTarget;

		public List< Port >						inputPorts = new List< Port >();
		public List< Port >						outputPorts = new List< Port >();

		protected Dictionary< string, Port >	portsPerFieldName = new Dictionary< string, Port >();

		protected BaseGraphView					owner;

        protected VisualElement 				controlsContainer;

		public void Initialize(BaseGraphView owner, BaseNode node)
		{
			nodeTarget = node;
			this.owner = owner;
			
			AddStyleSheetPath("GraphProcessorStyles/BaseNodeView");
			
            controlsContainer = new VisualElement{ name = "controls" };
        	mainContainer.Add(controlsContainer);

			InitializePorts();

			InitializeView();

			Enable();

			this.RefreshPorts();
		}

		public void AddPort(PortView p)
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

			portsPerFieldName[p.fieldName] = p;
		}

		public void RemovePort(PortView p)
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

			portsPerFieldName.Remove(p.fieldName);
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

				PortView port = new PortView(
					Orientation.Horizontal,
					(inputAttribute != null) ? Direction.Input : Direction.Output,
					field,
					owner.connectorListener
				);

				if (!String.IsNullOrEmpty(inputAttribute?.name))
					port.portName = inputAttribute.name;
				else if (!String.IsNullOrEmpty(outputAttribute?.name))
					port.portName = outputAttribute.name;

				AddPort(port);
			}
		}

		void InitializeView()
		{
			title = (string.IsNullOrEmpty(nodeTarget.name)) ? nodeTarget.GetType().Name : nodeTarget.name;

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
		}

		public virtual void DrawDefaultInspector()
		{
			var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			foreach (var field in fields)
			{
				//skip if the field is not serilizable
				if (!field.IsPublic && field.GetCustomAttribute(typeof(SerializeField)) == null)
					continue ;
				
				//skip if the field is an input/output
				if (field.GetCustomAttribute(typeof(InputAttribute)) != null || field.GetCustomAttribute(typeof(OutputAttribute)) != null)
					continue ;

                //skip if marked with NonSerialized
                if (field.GetCustomAttribute(typeof(System.NonSerializedAttribute)) != null)
                    continue ;

				var controlLabel = new Label(field.Name);
                controlsContainer.Add(controlLabel);

				var element = FieldFactory.CreateField(field, (newValue) => {
					field.SetValue(nodeTarget, newValue);
					owner.RegisterCompleteObjectUndo("Updated " + newValue);
				});

				controlsContainer.Add(element);
			}
		}

		public virtual void OnPortConnected(PortView port) {}
		public virtual void OnPortDisconnected(PortView port) {}

		public override void SetPosition(Rect newPos)
		{
			base.SetPosition(newPos);

			nodeTarget.position = newPos;
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

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendAction("Add to group", AddNodeToGroup, AddNodeToGroupStatus);
        }

        void AddNodeToGroup(ContextualMenu.MenuAction action)
        {
            Debug.Log("Attach node !");
        }

        StatusFlags AddNodeToGroupStatus(ContextualMenu.MenuAction action)
        {
            //FIXME !
            var mouseRect = new Rect(action.eventInfo.mousePosition, Vector2.zero);
            var block = owner.commentBlockViews.FirstOrDefault(c => c.Overlaps(mouseRect));

            return (block == null) ? StatusFlags.Disabled : StatusFlags.Normal;
        }
    }
}