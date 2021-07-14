using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections;
using System.Linq;
using UnityEditor.UIElements;
using System.Text.RegularExpressions;

using Status = UnityEngine.UIElements.DropdownMenuAction.Status;
using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace GraphProcessor
{
	[NodeCustomEditor(typeof(BaseNode))]
	public class BaseNodeView : NodeView
	{
		public BaseNode							nodeTarget;

		public List< PortView >					inputPortViews = new List< PortView >();
		public List< PortView >					outputPortViews = new List< PortView >();

		public BaseGraphView					owner { private set; get; }

		protected Dictionary< string, List< PortView > > portsPerFieldName = new Dictionary< string, List< PortView > >();

        public VisualElement 					controlsContainer;
		protected VisualElement					debugContainer;
		protected VisualElement					rightTitleContainer;
		protected VisualElement					topPortContainer;
		protected VisualElement					bottomPortContainer;
		private VisualElement 					inputContainerElement;

		VisualElement							settings;
		NodeSettingsView						settingsContainer;
		Button									settingButton;
		TextField								titleTextField;

		Label									computeOrderLabel = new Label();

		public event Action< PortView >			onPortConnected;
		public event Action< PortView >			onPortDisconnected;

		protected virtual bool					hasSettings { get; set; }

        public bool								initializing = false; //Used for applying SetPosition on locked node at init.

        readonly string							baseNodeStyle = "GraphProcessorStyles/BaseNodeView";

		bool									settingsExpanded = false;

		[System.NonSerialized]
		List< IconBadge >						badges = new List< IconBadge >();

		private List<Node> selectedNodes = new List<Node>();
		private float      selectedNodesFarLeft;
		private float      selectedNodesNearLeft;
		private float      selectedNodesFarRight;
		private float      selectedNodesNearRight;
		private float      selectedNodesFarTop;
		private float      selectedNodesNearTop;
		private float      selectedNodesFarBottom;
		private float      selectedNodesNearBottom;
		private float      selectedNodesAvgHorizontal;
		private float      selectedNodesAvgVertical;
		
		#region  Initialization
		
		public void Initialize(BaseGraphView owner, BaseNode node)
		{
			nodeTarget = node;
			this.owner = owner;

			if (!node.deletable)
				capabilities &= ~Capabilities.Deletable;
			// Note that the Renamable capability is useless right now as it haven't been implemented in Graphview
			if (node.isRenamable)
				capabilities |= Capabilities.Renamable;

			owner.computeOrderUpdated += ComputeOrderUpdatedCallback;
			node.onMessageAdded += AddMessageView;
			node.onMessageRemoved += RemoveMessageView;
			node.onPortsUpdated += a => schedule.Execute(_ => UpdatePortsForField(a)).ExecuteLater(0);

            styleSheets.Add(Resources.Load<StyleSheet>(baseNodeStyle));

            if (!string.IsNullOrEmpty(node.layoutStyle))
                styleSheets.Add(Resources.Load<StyleSheet>(node.layoutStyle));

			InitializeView();
			InitializePorts();
			InitializeDebug();

			// If the standard Enable method is still overwritten, we call it
			if (GetType().GetMethod(nameof(Enable), new Type[]{}).DeclaringType != typeof(BaseNodeView))
				ExceptionToLog.Call(() => Enable());
			else
				ExceptionToLog.Call(() => Enable(false));

			InitializeSettings();

			RefreshExpandedState();

			this.RefreshPorts();

			RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			RegisterCallback<DetachFromPanelEvent>(e => ExceptionToLog.Call(Disable));
			OnGeometryChanged(null);
		}

		void InitializePorts()
		{
			var listener = owner.connectorListener;

			foreach (var inputPort in nodeTarget.inputPorts)
			{
				AddPort(inputPort.fieldInfo, Direction.Input, listener, inputPort.portData);
			}

			foreach (var outputPort in nodeTarget.outputPorts)
			{
				AddPort(outputPort.fieldInfo, Direction.Output, listener, outputPort.portData);
			}
		}

		void InitializeView()
		{
            controlsContainer = new VisualElement{ name = "controls" };
			controlsContainer.AddToClassList("NodeControls");
			mainContainer.Add(controlsContainer);

			rightTitleContainer = new VisualElement{ name = "RightTitleContainer" };
			titleContainer.Add(rightTitleContainer);

			topPortContainer = new VisualElement { name = "TopPortContainer" };
			this.Insert(0, topPortContainer);

			bottomPortContainer = new VisualElement { name = "BottomPortContainer" };
			this.Add(bottomPortContainer);

			if (nodeTarget.showControlsOnHover)
			{
				bool mouseOverControls = false;
				controlsContainer.style.display = DisplayStyle.None;
				RegisterCallback<MouseOverEvent>(e => {
					controlsContainer.style.display = DisplayStyle.Flex;
					mouseOverControls = true;
				});
				RegisterCallback<MouseOutEvent>(e => {
					var rect = GetPosition();
					var graphMousePosition = owner.contentViewContainer.WorldToLocal(e.mousePosition);
					if (rect.Contains(graphMousePosition) || !nodeTarget.showControlsOnHover)
						return;
					mouseOverControls = false;
					schedule.Execute(_ => {
						if (!mouseOverControls)
							controlsContainer.style.display = DisplayStyle.None;
					}).ExecuteLater(500);
				});
			}

			Undo.undoRedoPerformed += UpdateFieldValues;

			debugContainer = new VisualElement{ name = "debug" };
			if (nodeTarget.debug)
				mainContainer.Add(debugContainer);

			initializing = true;

			UpdateTitle();
            SetPosition(nodeTarget.position);
			SetNodeColor(nodeTarget.color);
            
			AddInputContainer();

			// Add renaming capability
			if ((capabilities & Capabilities.Renamable) != 0)
				SetupRenamableTitle();
		}

		void SetupRenamableTitle()
		{
			var titleLabel = this.Q("title-label") as Label;

			titleTextField = new TextField{ isDelayed = true };
			titleTextField.style.display = DisplayStyle.None;
			titleLabel.parent.Insert(0, titleTextField);

			titleLabel.RegisterCallback<MouseDownEvent>(e => {
				if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
					OpenTitleEditor();
			});

			titleTextField.RegisterValueChangedCallback(e => CloseAndSaveTitleEditor(e.newValue));

			titleTextField.RegisterCallback<MouseDownEvent>(e => {
				if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
					CloseAndSaveTitleEditor(titleTextField.value);
			});

			titleTextField.RegisterCallback<FocusOutEvent>(e => CloseAndSaveTitleEditor(titleTextField.value));

			void OpenTitleEditor()
			{
				// show title textbox
				titleTextField.style.display = DisplayStyle.Flex;
				titleLabel.style.display = DisplayStyle.None;
				titleTextField.focusable = true;

				titleTextField.SetValueWithoutNotify(title);
				titleTextField.Focus();
				titleTextField.SelectAll();
			}

			void CloseAndSaveTitleEditor(string newTitle)
			{
				owner.RegisterCompleteObjectUndo("Renamed node " + newTitle);
				nodeTarget.SetCustomName(newTitle);

				// hide title TextBox
				titleTextField.style.display = DisplayStyle.None;
				titleLabel.style.display = DisplayStyle.Flex;
				titleTextField.focusable = false;

				UpdateTitle();
			}
		}

		void UpdateTitle()
		{
			title = (nodeTarget.GetCustomName() == null) ? nodeTarget.GetType().Name : nodeTarget.GetCustomName();
		}

		void InitializeSettings()
		{
			// Initialize settings button:
			if (hasSettings)
			{
				CreateSettingButton();
				settingsContainer = new NodeSettingsView();
				settingsContainer.visible = false;
				settings = new VisualElement();
				// Add Node type specific settings
				settings.Add(CreateSettingsView());
				settingsContainer.Add(settings);
				Add(settingsContainer);
				
				var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

				foreach(var field in fields)
					if(field.GetCustomAttribute(typeof(SettingAttribute)) != null) 
						AddSettingField(field);
			}
		}

		void OnGeometryChanged(GeometryChangedEvent evt)
		{
			if (settingButton != null)
			{
				var settingsButtonLayout = settingButton.ChangeCoordinatesTo(settingsContainer.parent, settingButton.layout);
				settingsContainer.style.top = settingsButtonLayout.yMax - 18f;
				settingsContainer.style.left = settingsButtonLayout.xMin - layout.width + 20f;
			}
		}

		// Workaround for bug in GraphView that makes the node selection border way too big
		VisualElement selectionBorder, nodeBorder;
		internal void EnableSyncSelectionBorderHeight()
		{
			if (selectionBorder == null || nodeBorder == null)
			{
				selectionBorder = this.Q("selection-border");
				nodeBorder = this.Q("node-border");

				schedule.Execute(() => {
					selectionBorder.style.height = nodeBorder.localBound.height;
				}).Every(17);
			}
		}
		
		void CreateSettingButton()
		{
			settingButton = new Button(ToggleSettings){name = "settings-button"};
			settingButton.Add(new Image { name = "icon", scaleMode = ScaleMode.ScaleToFit });

			titleContainer.Add(settingButton);
		}

		void ToggleSettings()
		{
			settingsExpanded = !settingsExpanded;
			if (settingsExpanded)
				OpenSettings();
			else
				CloseSettings();
		}

		public void OpenSettings()
		{
			if (settingsContainer != null)
			{
				owner.ClearSelection();
				owner.AddToSelection(this);

				settingButton.AddToClassList("clicked");
				settingsContainer.visible = true;
				settingsExpanded = true;
			}
		}

		public void CloseSettings()
		{
			if (settingsContainer != null)
			{
				settingButton.RemoveFromClassList("clicked");
				settingsContainer.visible = false;
				settingsExpanded = false;
			}
		}

		void InitializeDebug()
		{
			ComputeOrderUpdatedCallback();
			debugContainer.Add(computeOrderLabel);
		}

		#endregion

		#region API

		public List< PortView > GetPortViewsFromFieldName(string fieldName)
		{
			List< PortView >	ret;

			portsPerFieldName.TryGetValue(fieldName, out ret);

			return ret;
		}

		public PortView GetFirstPortViewFromFieldName(string fieldName)
		{
			return GetPortViewsFromFieldName(fieldName)?.First();
		}

		public PortView GetPortViewFromFieldName(string fieldName, string identifier)
		{
			return GetPortViewsFromFieldName(fieldName)?.FirstOrDefault(pv => {
				return (pv.portData.identifier == identifier) || (String.IsNullOrEmpty(pv.portData.identifier) && String.IsNullOrEmpty(identifier));
			});
		}


		public PortView AddPort(FieldInfo fieldInfo, Direction direction, BaseEdgeConnectorListener listener, PortData portData)
		{
			PortView p = CreatePortView(direction, fieldInfo, portData, listener);

			if (p.direction == Direction.Input)
			{
				inputPortViews.Add(p);

				if (portData.vertical)
					topPortContainer.Add(p);
				else
					inputContainer.Add(p);
			}
			else
			{
				outputPortViews.Add(p);

				if (portData.vertical)
					bottomPortContainer.Add(p);
				else
					outputContainer.Add(p);
			}

			p.Initialize(this, portData?.displayName);

			List< PortView > ports;
			portsPerFieldName.TryGetValue(p.fieldName, out ports);
			if (ports == null)
			{
				ports = new List< PortView >();
				portsPerFieldName[p.fieldName] = ports;
			}
			ports.Add(p);

			return p;
		}

        protected virtual PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener listener)
        	=> PortView.CreatePortView(direction, fieldInfo, portData, listener);

        public void InsertPort(PortView portView, int index)
		{
			if (portView.direction == Direction.Input)
			{
				if (portView.portData.vertical)
					topPortContainer.Insert(index, portView);
				else
					inputContainer.Insert(index, portView);
			}
			else
			{
				if (portView.portData.vertical)
					bottomPortContainer.Insert(index, portView);
				else
					outputContainer.Insert(index, portView);
			}
		}

		public void RemovePort(PortView p)
		{
			// Remove all connected edges:
			var edgesCopy = p.GetEdges().ToList();
			foreach (var e in edgesCopy)
				owner.Disconnect(e, refreshPorts: false);

			if (p.direction == Direction.Input)
			{
				if (inputPortViews.Remove(p))
					p.RemoveFromHierarchy();
			}
			else
			{
				if (outputPortViews.Remove(p))
					p.RemoveFromHierarchy();
			}

			List< PortView > ports;
			portsPerFieldName.TryGetValue(p.fieldName, out ports);
			ports.Remove(p);
		}
		
		private void SetValuesForSelectedNodes()
		{
			selectedNodes = new List<Node>();
			owner.nodes.ForEach(node =>
			{
				if(node.selected) selectedNodes.Add(node);
			});

			if(selectedNodes.Count < 2) return; //	No need for any of the calculations below

			selectedNodesFarLeft   = int.MinValue;
			selectedNodesFarRight  = int.MinValue;
			selectedNodesFarTop    = int.MinValue;
			selectedNodesFarBottom = int.MinValue;

			selectedNodesNearLeft   = int.MaxValue;
			selectedNodesNearRight  = int.MaxValue;
			selectedNodesNearTop    = int.MaxValue;
			selectedNodesNearBottom = int.MaxValue;

			foreach(var selectedNode in selectedNodes)
			{
				var nodeStyle  = selectedNode.style;
				var nodeWidth  = selectedNode.localBound.size.x;
				var nodeHeight = selectedNode.localBound.size.y;

				if(nodeStyle.left.value.value > selectedNodesFarLeft) selectedNodesFarLeft                 = nodeStyle.left.value.value;
				if(nodeStyle.left.value.value + nodeWidth > selectedNodesFarRight) selectedNodesFarRight   = nodeStyle.left.value.value + nodeWidth;
				if(nodeStyle.top.value.value > selectedNodesFarTop) selectedNodesFarTop                    = nodeStyle.top.value.value;
				if(nodeStyle.top.value.value + nodeHeight > selectedNodesFarBottom) selectedNodesFarBottom = nodeStyle.top.value.value + nodeHeight;

				if(nodeStyle.left.value.value < selectedNodesNearLeft) selectedNodesNearLeft                 = nodeStyle.left.value.value;
				if(nodeStyle.left.value.value + nodeWidth < selectedNodesNearRight) selectedNodesNearRight   = nodeStyle.left.value.value + nodeWidth;
				if(nodeStyle.top.value.value < selectedNodesNearTop) selectedNodesNearTop                    = nodeStyle.top.value.value;
				if(nodeStyle.top.value.value + nodeHeight < selectedNodesNearBottom) selectedNodesNearBottom = nodeStyle.top.value.value + nodeHeight;
			}

			selectedNodesAvgHorizontal = (selectedNodesNearLeft + selectedNodesFarRight) / 2f;
			selectedNodesAvgVertical   = (selectedNodesNearTop + selectedNodesFarBottom) / 2f;
		}

		public static Rect GetNodeRect(Node node, float left = int.MaxValue, float top = int.MaxValue)
		{
			return new Rect(
				new Vector2(left != int.MaxValue ? left : node.style.left.value.value, top != int.MaxValue ? top : node.style.top.value.value),
				new Vector2(node.style.width.value.value, node.style.height.value.value)
			);
		}

		public void AlignToLeft()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesNearLeft));
			}
		}

		public void AlignToCenter()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesAvgHorizontal - selectedNode.localBound.size.x / 2f));
			}
		}

		public void AlignToRight()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesFarRight - selectedNode.localBound.size.x));
			}
		}

		public void AlignToTop()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesNearTop));
			}
		}

		public void AlignToMiddle()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesAvgVertical - selectedNode.localBound.size.y / 2f));
			}
		}

		public void AlignToBottom()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesFarBottom - selectedNode.localBound.size.y));
			}
		}

		public void OpenNodeViewScript()
		{
			var script = NodeProvider.GetNodeViewScript(GetType());

			if (script != null)
				AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
		}

		public void OpenNodeScript()
		{
			var script = NodeProvider.GetNodeScript(nodeTarget.GetType());

			if (script != null)
				AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
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

		public void AddMessageView(string message, Texture icon, Color color)
			=> AddBadge(new NodeBadgeView(message, icon, color));

		public void AddMessageView(string message, NodeMessageType messageType)
		{
			IconBadge	badge = null;
			switch (messageType)
			{
				case NodeMessageType.Warning:
					badge = new NodeBadgeView(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.yellow);
					break ;
				case NodeMessageType.Error:	
					badge = IconBadge.CreateError(message);
					break ;
				case NodeMessageType.Info:
					badge = IconBadge.CreateComment(message);
					break ;
				default:
				case NodeMessageType.None:
					badge = new NodeBadgeView(message, null, Color.grey);
					break ;
			}
			
			AddBadge(badge);
		}

		void AddBadge(IconBadge badge)
		{
			Add(badge);
			badges.Add(badge);
			badge.AttachTo(topContainer, SpriteAlignment.TopRight);
		}

		void RemoveBadge(Func<IconBadge, bool> callback)
		{
			badges.RemoveAll(b => {
				if (callback(b))
				{
					b.Detach();
					b.RemoveFromHierarchy();
					return true;
				}
				return false;
			});
		}

		public void RemoveMessageViewContains(string message) => RemoveBadge(b => b.badgeText.Contains(message));
		
		public void RemoveMessageView(string message) => RemoveBadge(b => b.badgeText == message);

		public void Highlight()
		{
			AddToClassList("Highlight");
		}

		public void UnHighlight()
		{
			RemoveFromClassList("Highlight");
		}

		#endregion

		#region Callbacks & Overrides

		void ComputeOrderUpdatedCallback()
		{
			//Update debug compute order
			computeOrderLabel.text = "Compute order: " + nodeTarget.computeOrder;
		}

		public virtual void Enable(bool fromInspector = false) => DrawDefaultInspector(fromInspector);
		public virtual void Enable() => DrawDefaultInspector(false);

		public virtual void Disable() {}

		Dictionary<string, List<(object value, VisualElement target)>> visibleConditions = new Dictionary<string, List<(object value, VisualElement target)>>();
		Dictionary<string, VisualElement>  hideElementIfConnected = new Dictionary<string, VisualElement>();
		Dictionary<FieldInfo, List<VisualElement>> fieldControlsMap = new Dictionary<FieldInfo, List<VisualElement>>();

		protected void AddInputContainer()
		{
			inputContainerElement = new VisualElement {name = "input-container"};
			mainContainer.parent.Add(inputContainerElement);
			inputContainerElement.SendToBack();
			inputContainerElement.pickingMode = PickingMode.Ignore;
		}

		protected virtual void DrawDefaultInspector(bool fromInspector = false)
		{
			var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				// Filter fields from the BaseNode type since we are only interested in user-defined fields
				// (better than BindingFlags.DeclaredOnly because we keep any inherited user-defined fields) 
				.Where(f => f.DeclaringType != typeof(BaseNode));

			fields = nodeTarget.OverrideFieldOrder(fields).Reverse();

			foreach (var field in fields)
			{
				//skip if the field is a node setting
				if(field.GetCustomAttribute(typeof(SettingAttribute)) != null)
				{
					hasSettings = true;
					continue;
				}

				//skip if the field is not serializable
				if((!field.IsPublic && field.GetCustomAttribute(typeof(SerializeField)) == null) || field.IsNotSerialized)
				{
					AddEmptyField(field, fromInspector);
					continue;
				}

				//skip if the field is an input/output and not marked as SerializedField
				bool hasInputAttribute         = field.GetCustomAttribute(typeof(InputAttribute)) != null;
				bool hasInputOrOutputAttribute = hasInputAttribute || field.GetCustomAttribute(typeof(OutputAttribute)) != null;
				bool showAsDrawer			   = !fromInspector && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
				if (field.GetCustomAttribute(typeof(SerializeField)) == null && hasInputOrOutputAttribute && !showAsDrawer)
				{
					AddEmptyField(field, fromInspector);
					continue;
				}

				//skip if marked with NonSerialized or HideInInspector
				if (field.GetCustomAttribute(typeof(System.NonSerializedAttribute)) != null || field.GetCustomAttribute(typeof(HideInInspector)) != null)
				{
					AddEmptyField(field, fromInspector);
					continue;
				}

				// Hide the field if we want to display in in the inspector
				var showInInspector = field.GetCustomAttribute<ShowInInspector>();
				if (showInInspector != null && !showInInspector.showInNode && !fromInspector)
				{
					AddEmptyField(field, fromInspector);
					continue;
				}

				var showInputDrawer = field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(SerializeField)) != null;
				showInputDrawer |= field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
				showInputDrawer &= !fromInspector; // We can't show a drawer in the inspector
				showInputDrawer &= !typeof(IList).IsAssignableFrom(field.FieldType);

				string displayName = ObjectNames.NicifyVariableName(field.Name);

				var inspectorNameAttribute = field.GetCustomAttribute<InspectorNameAttribute>();
				if (inspectorNameAttribute != null)
					displayName = inspectorNameAttribute.displayName;

				var elem = AddControlField(field, displayName, showInputDrawer);
				if (hasInputAttribute)
				{
					hideElementIfConnected[field.Name] = elem;

					// Hide the field right away if there is already a connection:
					if (portsPerFieldName.TryGetValue(field.Name, out var pvs))
						if (pvs.Any(pv => pv.GetEdges().Count > 0))
							elem.style.display = DisplayStyle.None;
				}
			}
		}

		protected virtual void SetNodeColor(Color color)
		{
			titleContainer.style.borderBottomColor = new StyleColor(color);
			titleContainer.style.borderBottomWidth = new StyleFloat(color.a > 0 ? 5f : 0f);
		}
		
		private void AddEmptyField(FieldInfo field, bool fromInspector)
		{
			if (field.GetCustomAttribute(typeof(InputAttribute)) == null || fromInspector)
				return;

			if (field.GetCustomAttribute<VerticalAttribute>() != null)
				return;
			
			var box = new VisualElement {name = field.Name};
			box.AddToClassList("port-input-element");
			box.AddToClassList("empty");
			inputContainerElement.Add(box);
		}

		void UpdateFieldVisibility(string fieldName, object newValue)
		{
			if (newValue == null)
				return;
			if (visibleConditions.TryGetValue(fieldName, out var list))
			{
				foreach (var elem in list)
				{
					if (newValue.Equals(elem.value))
						elem.target.style.display = DisplayStyle.Flex;
					else
						elem.target.style.display = DisplayStyle.None;
				}
			}
		}

		void UpdateOtherFieldValueSpecific<T>(FieldInfo field, object newValue)
		{
			foreach (var inputField in fieldControlsMap[field])
			{
				var notify = inputField as INotifyValueChanged<T>;
				if (notify != null)
					notify.SetValueWithoutNotify((T)newValue);
			}
		}

		static MethodInfo specificUpdateOtherFieldValue = typeof(BaseNodeView).GetMethod(nameof(UpdateOtherFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
		void UpdateOtherFieldValue(FieldInfo info, object newValue)
		{
			// Warning: Keep in sync with FieldFactory CreateField
			var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
			var genericUpdate = specificUpdateOtherFieldValue.MakeGenericMethod(fieldType);

			genericUpdate.Invoke(this, new object[]{info, newValue});
		}

		object GetInputFieldValueSpecific<T>(FieldInfo field)
		{
			if (fieldControlsMap.TryGetValue(field, out var list))
			{
				foreach (var inputField in list)
				{
					if (inputField is INotifyValueChanged<T> notify)
						return notify.value;
				}
			}
			return null;
		}

		static MethodInfo specificGetValue = typeof(BaseNodeView).GetMethod(nameof(GetInputFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
		object GetInputFieldValue(FieldInfo info)
		{
			// Warning: Keep in sync with FieldFactory CreateField
			var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
			var genericUpdate = specificGetValue.MakeGenericMethod(fieldType);

			return genericUpdate.Invoke(this, new object[]{info});
		}

		protected VisualElement AddControlField(string fieldName, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
			=> AddControlField(nodeTarget.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), label, showInputDrawer, valueChangedCallback);

		Regex s_ReplaceNodeIndexPropertyPath = new Regex(@"(^nodes.Array.data\[)(\d+)(\])");
		internal void SyncSerializedPropertyPathes()
		{
			int nodeIndex = owner.graph.nodes.FindIndex(n => n == nodeTarget);

			// If the node is not found, then it means that it has been deleted from serialized data.
			if (nodeIndex == -1)
				return;

			var nodeIndexString = nodeIndex.ToString();
			foreach (var propertyField in this.Query<PropertyField>().ToList())
			{
				propertyField.Unbind();
				// The property path look like this: nodes.Array.data[x].fieldName
				// And we want to update the value of x with the new node index:
				propertyField.bindingPath = s_ReplaceNodeIndexPropertyPath.Replace(propertyField.bindingPath, m => m.Groups[1].Value + nodeIndexString + m.Groups[3].Value);
				propertyField.Bind(owner.serializedGraph);
			}
		}

		protected SerializedProperty FindSerializedProperty(string fieldName)
		{
			int i = owner.graph.nodes.FindIndex(n => n == nodeTarget);
			return owner.serializedGraph.FindProperty("nodes").GetArrayElementAtIndex(i).FindPropertyRelative(fieldName);
		}

		protected VisualElement AddControlField(FieldInfo field, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
		{
			if (field == null)
				return null;

			var element = new PropertyField(FindSerializedProperty(field.Name), showInputDrawer ? "" : label);
			element.Bind(owner.serializedGraph);

#if UNITY_2020_3 // In Unity 2020.3 the empty label on property field doesn't hide it, so we do it manually
			if ((showInputDrawer || String.IsNullOrEmpty(label)) && element != null)
				element.AddToClassList("DrawerField_2020_3");
#endif

			if (typeof(IList).IsAssignableFrom(field.FieldType))
				EnableSyncSelectionBorderHeight();

			element.RegisterValueChangeCallback(e => {
				UpdateFieldVisibility(field.Name, field.GetValue(nodeTarget));
				valueChangedCallback?.Invoke();
				NotifyNodeChanged();
			});

			// Disallow picking scene objects when the graph is not linked to a scene
			if (element != null && !owner.graph.IsLinkedToScene())
			{
				var objectField = element.Q<ObjectField>();
				if (objectField != null)
					objectField.allowSceneObjects = false;
			}

			if (!fieldControlsMap.TryGetValue(field, out var inputFieldList))
				inputFieldList = fieldControlsMap[field] = new List<VisualElement>();
			inputFieldList.Add(element);

			if(element != null)
			{
				if (showInputDrawer)
				{
					var box = new VisualElement {name = field.Name};
					box.AddToClassList("port-input-element");
					box.Add(element);
					inputContainerElement.Add(box);
				}
				else
				{
					controlsContainer.Add(element);
				}
				element.name = field.Name;
			}
			else
			{
				// Make sure we create an empty placeholder if FieldFactory can not provide a drawer
				if (showInputDrawer) AddEmptyField(field, false);
			}

			var visibleCondition = field.GetCustomAttribute(typeof(VisibleIf)) as VisibleIf;
			if (visibleCondition != null)
			{
				// Check if target field exists:
				var conditionField = nodeTarget.GetType().GetField(visibleCondition.fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				if (conditionField == null)
					Debug.LogError($"[VisibleIf] Field {visibleCondition.fieldName} does not exists in node {nodeTarget.GetType()}");
				else
				{
					visibleConditions.TryGetValue(visibleCondition.fieldName, out var list);
					if (list == null)
						list = visibleConditions[visibleCondition.fieldName] = new List<(object value, VisualElement target)>();
					list.Add((visibleCondition.value, element));
					UpdateFieldVisibility(visibleCondition.fieldName, conditionField.GetValue(nodeTarget));
				}
			}

			return element;
		}

		void UpdateFieldValues()
		{
			foreach (var kp in fieldControlsMap)
				UpdateOtherFieldValue(kp.Key, kp.Key.GetValue(nodeTarget));
		}
		
		protected void AddSettingField(FieldInfo field)
		{
			if (field == null)
				return;

			var label = field.GetCustomAttribute<SettingAttribute>().name;

			var element = new PropertyField(FindSerializedProperty(field.Name));
			element.Bind(owner.serializedGraph);

			if (element != null)
			{
				settingsContainer.Add(element);
				element.name = field.Name;
			}
		}

		internal void OnPortConnected(PortView port)
		{
			if(port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
				inputContainerElement.Q(port.fieldName).AddToClassList("empty");
			
			if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))
				elem.style.display = DisplayStyle.None;

			onPortConnected?.Invoke(port);
		}

		internal void OnPortDisconnected(PortView port)
		{
			if (port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
			{
				inputContainerElement.Q(port.fieldName).RemoveFromClassList("empty");

				if (nodeTarget.nodeFields.TryGetValue(port.fieldName, out var fieldInfo))
				{
					var valueBeforeConnection = GetInputFieldValue(fieldInfo.info);

					if (valueBeforeConnection != null)
					{
						fieldInfo.info.SetValue(nodeTarget, valueBeforeConnection);
					}
				}
			}
			
			if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))
				elem.style.display = DisplayStyle.Flex;

			onPortDisconnected?.Invoke(port);
		}

		// TODO: a function to force to reload the custom behavior ports (if we want to do a button to add ports for example)

		public virtual void OnRemoved() {}
		public virtual void OnCreated() {}

		public override void SetPosition(Rect newPos)
		{
            if (initializing || !nodeTarget.isLocked)
            {
                base.SetPosition(newPos);

				if (!initializing)
					owner.RegisterCompleteObjectUndo("Moved graph node");

                nodeTarget.position = newPos;
                initializing = false;
            }
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

        public void ChangeLockStatus()
        {
            nodeTarget.nodeLock ^= true;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			BuildAlignMenu(evt);
			evt.menu.AppendAction("Open Node Script", (e) => OpenNodeScript(), OpenNodeScriptStatus);
			evt.menu.AppendAction("Open Node View Script", (e) => OpenNodeViewScript(), OpenNodeViewScriptStatus);
			evt.menu.AppendAction("Debug", (e) => ToggleDebug(), DebugStatus);
            if (nodeTarget.unlockable)
                evt.menu.AppendAction((nodeTarget.isLocked ? "Unlock" : "Lock"), (e) => ChangeLockStatus(), LockStatus);
        }

		protected void BuildAlignMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Align/To Left", (e) => AlignToLeft());
			evt.menu.AppendAction("Align/To Center", (e) => AlignToCenter());
			evt.menu.AppendAction("Align/To Right", (e) => AlignToRight());
			evt.menu.AppendSeparator("Align/");
			evt.menu.AppendAction("Align/To Top", (e) => AlignToTop());
			evt.menu.AppendAction("Align/To Middle", (e) => AlignToMiddle());
			evt.menu.AppendAction("Align/To Bottom", (e) => AlignToBottom());
			evt.menu.AppendSeparator();
		}

        Status LockStatus(DropdownMenuAction action)
        {
            return Status.Normal;
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

		IEnumerable< PortView > SyncPortCounts(IEnumerable< NodePort > ports, IEnumerable< PortView > portViews)
		{
			var listener = owner.connectorListener;
			var portViewList = portViews.ToList();

			// Maybe not good to remove ports as edges are still connected :/
			foreach (var pv in portViews.ToList())
			{
				// If the port have disappeared from the node data, we remove the view:
				// We can use the identifier here because this function will only be called when there is a custom port behavior
				if (!ports.Any(p => p.portData.identifier == pv.portData.identifier))
				{
					RemovePort(pv);
					portViewList.Remove(pv);
				}
			}

			foreach (var p in ports)
			{
				// Add missing port views
				if (!portViews.Any(pv => p.portData.identifier == pv.portData.identifier))
				{
					Direction portDirection = nodeTarget.IsFieldInput(p.fieldName) ? Direction.Input : Direction.Output;
					var pv = AddPort(p.fieldInfo, portDirection, listener, p.portData);
					portViewList.Add(pv);
				}
			}

			return portViewList;
		}

		void SyncPortOrder(IEnumerable< NodePort > ports, IEnumerable< PortView > portViews)
		{
			var portViewList = portViews.ToList();
			var portsList = ports.ToList();

			// Re-order the port views to match the ports order in case a custom behavior re-ordered the ports
			for (int i = 0; i < portsList.Count; i++)
			{
				var id = portsList[i].portData.identifier;

				var pv = portViewList.FirstOrDefault(p => p.portData.identifier == id);
				if (pv != null)
					InsertPort(pv, i);
			}
		}

		public virtual new bool RefreshPorts()
		{
			// If a port behavior was attached to one port, then
			// the port count might have been updated by the node
			// so we have to refresh the list of port views.
			UpdatePortViewWithPorts(nodeTarget.inputPorts, inputPortViews);
			UpdatePortViewWithPorts(nodeTarget.outputPorts, outputPortViews);

			void UpdatePortViewWithPorts(NodePortContainer ports, List< PortView > portViews)
			{
				if (ports.Count == 0 && portViews.Count == 0) // Nothing to update
					return;

				// When there is no current portviews, we can't zip the list so we just add all
				if (portViews.Count == 0)
					SyncPortCounts(ports, new PortView[]{});
				else if (ports.Count == 0) // Same when there is no ports
					SyncPortCounts(new NodePort[]{}, portViews);
				else if (portViews.Count != ports.Count)
					SyncPortCounts(ports, portViews);
				else
				{
					var p = ports.GroupBy(n => n.fieldName);
					var pv = portViews.GroupBy(v => v.fieldName);
					p.Zip(pv, (portPerFieldName, portViewPerFieldName) => {
						IEnumerable< PortView > portViewsList = portViewPerFieldName;
						if (portPerFieldName.Count() != portViewPerFieldName.Count())
							portViewsList = SyncPortCounts(portPerFieldName, portViewPerFieldName);
						SyncPortOrder(portPerFieldName, portViewsList);
						// We don't care about the result, we just iterate over port and portView
						return "";
					}).ToList();
				}

				// Here we're sure that we have the same amount of port and portView
				// so we can update the view with the new port data (if the name of a port have been changed for example)

				for (int i = 0; i < portViews.Count; i++)
					portViews[i].UpdatePortView(ports[i].portData);
			}

			return base.RefreshPorts();
		}

		public void ForceUpdatePorts()
		{
			nodeTarget.UpdateAllPorts();

			RefreshPorts();
		}

		void UpdatePortsForField(string fieldName)
		{
			// TODO: actual code
			RefreshPorts();
		}

		protected virtual VisualElement CreateSettingsView() => new Label("Settings") {name = "header"};

		/// <summary>
		/// Send an event to the graph telling that the content of this node have changed
		/// </summary>
		public void NotifyNodeChanged() => owner.graph.NotifyNodeChanged(nodeTarget);

		#endregion
    }
}