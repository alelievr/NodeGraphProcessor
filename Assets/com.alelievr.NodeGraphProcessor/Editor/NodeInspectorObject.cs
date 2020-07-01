using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace GraphProcessor
{
    [CustomEditor(typeof(NodeInspectorObject))]
    public class NodeInspectorObjectEditor : Editor
    {
        NodeInspectorObject inspector;
        VisualElement root;

        VisualElement placeholder;

        Dictionary<BaseNode, VisualElement> nodeInspectorCache = new Dictionary<BaseNode, VisualElement>();

        void OnEnable()
        {
            inspector = target as NodeInspectorObject;
            inspector.nodeSelectionUpdated += UpdateNodeInspector;
            root = new VisualElement();
            placeholder = new Label("Select a node to show it's settings in the inspector");
            placeholder.AddToClassList("PlaceHolder");
            UpdateNodeInspector();
        }

        public override VisualElement CreateInspectorGUI() => root;

        void UpdateNodeInspector()
        {
            root.styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessorStyles/InspectorView"));
            root.Clear();

            if (inspector.selectedNodes.Count == 0)
                root.Add(placeholder);

            foreach (var nodeView in inspector.selectedNodes)
            {
                var tmp = nodeView.controlsContainer;
                nodeView.controlsContainer = CreateNodeVisualElement(nodeView);
                nodeView.Enable(true);
                nodeView.controlsContainer.AddToClassList("NodeControls");
                root.Add(nodeView.controlsContainer);
                nodeView.controlsContainer = tmp;
            }
        }

        VisualElement CreateNodeVisualElement(BaseNodeView nodeView)
        {
            var view = new VisualElement();

            view.AddToClassList("");
            view.Add(new Label(nodeView.nodeTarget.name));
            
            return view;
        }
    }

    public class NodeInspectorObject : ScriptableObject
    {
        public Object previouslySelectedObject;
        public HashSet<BaseNodeView> selectedNodes { get; private set; } = new HashSet<BaseNodeView>();

        internal event Action nodeSelectionUpdated;

        public void UpdateSelectedNodes(HashSet<BaseNodeView> views)
        {
            selectedNodes = views;
            nodeSelectionUpdated?.Invoke();
        }
    }
}