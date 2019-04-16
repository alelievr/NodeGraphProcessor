using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;
using UnityEditorInternal;

using Status = UnityEngine.UIElements.DropdownMenuAction.Status;
using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace GraphProcessor
{
	[NodeCustomEditor(typeof(BaseNode))]
	public class BaseNodeView : NodeView
	{
		public BaseNode							nodeTarget;

		public List< Port >						inputPorts = new List< Port >();
		public List< Port >						outputPorts = new List< Port >();

		public BaseGraphView					owner { private set; get; }

		protected Dictionary< string, PortView >portsPerFieldName = new Dictionary< string, PortView >();

        protected VisualElement 				controlsContainer;
		protected VisualElement					debugContainer;

		Label									computeOrderLabel = new Label();

		readonly string							baseNodeStyle = "GraphProcessorStyles/BaseNodeView";

		#region  Initialization

		public void Initialize(BaseGraphView owner, BaseNode node)
		{
			nodeTarget = node;
			this.owner = owner;

			owner.computeOrderUpdated += ComputeOrderUpdatedCallback;

			styleSheets.Add(Resources.Load<StyleSheet>(baseNodeStyle));

			InitializePorts();
			InitializeView();
			InitializeDebug();

			Enable();

			this.RefreshPorts();
		}

		void InitializePorts()
		{
			foreach (var fieldInfo in nodeTarget.nodeFields)
			{
				// This will automatically create our visual ports
				PortBehaviorFactory.CreatePortBehavior(
					this,
					fieldInfo.Value.info,
					fieldInfo.Value.input ? Direction.Input : Direction.Output,
					owner.connectorListener,
					fieldInfo.Value.isMultiple,
					fieldInfo.Value.name
				);
			}
		}

		void InitializeView()
		{
            controlsContainer = new VisualElement{ name = "controls" };
			mainContainer.Add(controlsContainer);

			debugContainer = new VisualElement{ name = "debug" };
			if (nodeTarget.debug)
				mainContainer.Add(debugContainer);

			title = (string.IsNullOrEmpty(nodeTarget.name)) ? nodeTarget.GetType().Name : nodeTarget.name;

			SetPosition(nodeTarget.position);
		}

		void InitializeDebug()
		{
			ComputeOrderUpdatedCallback();
			debugContainer.Add(computeOrderLabel);
		}

		#endregion

		#region API

		public PortView GetPortFromFieldName(string fieldName)
		{
			PortView	ret;

			portsPerFieldName.TryGetValue(fieldName, out ret);

			return ret;
		}

		public PortView AddPort(FieldInfo fieldInfo, Direction direction, EdgeConnectorListener listener, bool isMultiple = false, string name = null)
		{
			// TODO: hardcoded value
			PortView p = new PortView(Orientation.Horizontal, direction, fieldInfo, listener);

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

			p.Initialize(this, isMultiple, name);

			portsPerFieldName[p.fieldName] = p;

			return p;
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

		public void OpenNodeViewScript()
		{
			var scriptPath = NodeProvider.GetNodeViewScript(GetType());

			if (scriptPath != null)
				InternalEditorUtility.OpenFileAtLineExternal(scriptPath, 0);
		}

		public void OpenNodeScript()
		{
			var scriptPath = NodeProvider.GetNodeScript(nodeTarget.GetType());

			if (scriptPath != null)
				InternalEditorUtility.OpenFileAtLineExternal(scriptPath, 0);
		}

		public void ToggleDebug()
		{
			nodeTarget.debug = !nodeTarget.debug;
			UpdateDebugView();
		}

		public void UpdateDebugView()
		{
			if (nodeTarget.debug)
				mainContainer.Add(debugContainer);
			else
				mainContainer.Remove(debugContainer);
		}

		#endregion

		#region Callbacks & Overrides

		void ComputeOrderUpdatedCallback()
		{
			//Update debug compute order
			computeOrderLabel.text = "Compute order: " + nodeTarget.computeOrder;
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
				//skip if the field is not serializable
				if (!field.IsPublic && field.GetCustomAttribute(typeof(SerializeField)) == null)
					continue ;

				//skip if the field is an input/output and not marked as SerializedField
				if (field.GetCustomAttribute(typeof(SerializeField)) == null && (field.GetCustomAttribute(typeof(InputAttribute)) != null || field.GetCustomAttribute(typeof(OutputAttribute)) != null))
					continue ;

                //skip if marked with NonSerialized or HideInInspector
                if (field.GetCustomAttribute(typeof(System.NonSerializedAttribute)) != null || field.GetCustomAttribute(typeof(HideInInspector)) != null)
                    continue ;

				var controlLabel = new Label(field.Name);
                controlsContainer.Add(controlLabel);

				var element = FieldFactory.CreateField(field.FieldType, field.GetValue(nodeTarget), (newValue) => {
					owner.RegisterCompleteObjectUndo("Updated " + newValue);
					field.SetValue(nodeTarget, newValue);
				});

				if (element != null)
					controlsContainer.Add(element);
			}
		}

		public virtual void OnPortConnected(PortView port) {}
		public virtual void OnPortDisconnected(PortView port) {}

		public virtual void OnRemoved() {}
		public virtual void OnCreated() {}

		public override void SetPosition(Rect newPos)
		{
			base.SetPosition(newPos);

			Undo.RegisterCompleteObjectUndo(owner.graph, "Moved graph node");
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
			evt.menu.AppendAction("Open Node Script", (e) => OpenNodeScript(), OpenNodeScriptStatus);
			evt.menu.AppendAction("Open Node View Script", (e) => OpenNodeViewScript(), OpenNodeViewScriptStatus);
			evt.menu.AppendAction("Debug", (e) => ToggleDebug(), DebugStatus);
		}

		Status DebugStatus(DropdownMenuAction action)
		{
			if (nodeTarget.debug)
				return Status.Checked;
			return Status.Normal;
		}

		Status OpenNodeScriptStatus(DropdownMenuAction action)
		{
			if (NodeProvider.GetNodeScript(nodeTarget.GetType()) != null)
				return Status.Normal;
			return Status.Disabled;
		}

		Status OpenNodeViewScriptStatus(DropdownMenuAction action)
		{
			if (NodeProvider.GetNodeViewScript(GetType()) != null)
				return Status.Normal;
			return Status.Disabled;
		}

		#endregion
    }
}