using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;
using UnityEditorInternal;

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

		public BaseGraphView					owner { private set; get; }

		protected Dictionary< string, Port >	portsPerFieldName = new Dictionary< string, Port >();

        protected VisualElement 				controlsContainer;
		protected VisualElement					debugContainer;

		Label									computeOrderLabel;

		#region  Initialization

		public void Initialize(BaseGraphView owner, BaseNode node)
		{
			nodeTarget = node;
			this.owner = owner;

			owner.computeOrderUpdated += ComputeOrderUpdatedCallback;
			
			AddStyleSheetPath("GraphProcessorStyles/BaseNodeView");

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
				AddPort(
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
			mainContainer.Add(debugContainer);

			title = (string.IsNullOrEmpty(nodeTarget.name)) ? nodeTarget.GetType().Name : nodeTarget.name;

			SetPosition(nodeTarget.position);
		}

		void InitializeDebug()
		{
			computeOrderLabel = new Label("compute order: " + nodeTarget.computeOrder);
			debugContainer.Add(computeOrderLabel);
		}

		#endregion
		
		#region API

		public Port GetPortFromFieldName(string fieldName)
		{
			Port	ret;

			portsPerFieldName.TryGetValue(fieldName, out ret);

			return ret;
		}

		public PortView AddPort(FieldInfo fieldInfo, Direction direction, EdgeConnectorListener listener, bool isMultiple = false, string name = null)
		{
			PortView p = PortViewFactory.Create(fieldInfo, direction, listener);

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
			
			Debug.Log("Add port: " + p.fieldName + (p as MultiPortView)?.id);

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
				Debug.Log("Remove port: " + p.fieldName + (p as MultiPortView)?.id);
				
			}
			else
			{
				Debug.Log("Remove port: " + p.fieldName + (p as MultiPortView)?.id);
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

				if (element != null)
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
			evt.menu.AppendAction("Open Node Script", (e) => OpenNodeScript(), OpenNodeScriptStatus);
			evt.menu.AppendAction("Open Node View Script", (e) => OpenNodeViewScript(), OpenNodeViewScriptStatus);
		}

		StatusFlags OpenNodeScriptStatus(ContextualMenu.MenuAction action)
		{
			if (NodeProvider.GetNodeScript(nodeTarget.GetType()) != null)
				return StatusFlags.Normal;
			return StatusFlags.Disabled;
		}
		
		StatusFlags OpenNodeViewScriptStatus(ContextualMenu.MenuAction action)
		{
			if (NodeProvider.GetNodeViewScript(GetType()) != null)
				return StatusFlags.Normal;
			return StatusFlags.Disabled;
		}

		#endregion
    }
}