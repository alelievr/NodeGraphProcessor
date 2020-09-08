using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace GraphProcessor
{
    // TODO: replace this by the new UnityEditor.Searcher package
    public class CreateNodeMenuWindow : ScriptableObject, ISearchWindowProvider
    {
        protected BaseGraphView   graphView;
        EditorWindow    window;
        protected Texture2D       icon;
        EdgeView        edgeFilter;
        PortView        inputPortView;
        PortView        outputPortView;

        public void Initialize(BaseGraphView graphView, EditorWindow window, EdgeView edgeFilter = null)
        {
            this.graphView = graphView;
            this.window = window;
            this.edgeFilter = edgeFilter;
            this.inputPortView = edgeFilter?.input as PortView;
            this.outputPortView = edgeFilter?.output as PortView;

            // Transparent icon to trick search window into indenting items
            if (icon == null)
                icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            icon.Apply();
        }

        void OnDestroy()
        {
            if (icon != null)
            {
                DestroyImmediate(icon);
                icon = null;
            }
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };

            // Sort menu by alphabetical order and submenus
            if (edgeFilter == null)
                CreateStandardNodeMenu(tree);
            else
                CreateEdgeNodeMenu(tree);

            return tree;
        }

        void CreateStandardNodeMenu(List<SearchTreeEntry> tree)
        {
            var nodeEntries = graphView.FilterCreateNodeMenuEntries().OrderBy(k => k.Key);
            var titlePaths = new HashSet< string >();
            
			foreach (var nodeMenuItem in nodeEntries)
			{
                var nodePath = nodeMenuItem.Key;
                var nodeName = nodePath;
                var level    = 0;
                var parts    = nodePath.Split('/');

                if(parts.Length > 1)
                {
                    level++;
                    nodeName = parts[parts.Length - 1];
                    var fullTitleAsPath = "";
                    
                    for(var i = 0; i < parts.Length - 1; i++)
                    {
                        var title = parts[i];
                        fullTitleAsPath += title;
                        level = i + 1;
                        
                        // Add section title if the node is in subcategory
                        if (!titlePaths.Contains(fullTitleAsPath))
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(title)){
                                level = level
                            });
                            titlePaths.Add(fullTitleAsPath);
                        }
                    }
                }

                tree.Add(AddStandardSearchEntry(nodeMenuItem, nodeName, level));
            }
        }
       
        protected virtual SearchTreeEntry AddStandardSearchEntry(KeyValuePair<string, Type> nodeMenuItem, string nodeName, int level)
        {
            return new SearchTreeEntry(new GUIContent(nodeName, icon))
            {
                level = level + 1,
                userData = nodeMenuItem.Value
            };
        }

        protected virtual void CreateEdgeNodeMenu(List<SearchTreeEntry> tree)
        {
            var entries = NodeProvider.GetEdgeCreationNodeMenuEntry((edgeFilter.input ?? edgeFilter.output) as PortView);

            var titlePaths = new HashSet< string >();

            var nodePaths = NodeProvider.GetNodeMenuEntries();

            tree.Add(new SearchTreeEntry(new GUIContent($"Relay", icon))
            {
                level = 1,
                userData = new NodeProvider.PortDescription{
			        nodeType = typeof(RelayNode),
			        portType = typeof(System.Object),
			        isInput = inputPortView != null,
			        portFieldName = inputPortView != null ? nameof(RelayNode.output) : nameof(RelayNode.input),
			        portIdentifier = "0",
			        portDisplayName = inputPortView != null ? "Out" : "In",
                }
            });

			foreach (var nodeMenuItem in entries.OrderBy(n => n.nodeType.ToString()))
            {
                var nodePath = GetEdgeNodePath(nodePaths, nodeMenuItem);

                // Ignore the node if it's not in the create menu
                if (String.IsNullOrEmpty(nodePath))
                    continue;

                var nodeName = nodePath;
                var level = 0;
                var parts = nodePath.Split('/');

                if (parts.Length > 1)
                {
                    level++;
                    nodeName = parts[parts.Length - 1];
                    var fullTitleAsPath = "";

                    for (var i = 0; i < parts.Length - 1; i++)
                    {
                        var title = parts[i];
                        fullTitleAsPath += title;
                        level = i + 1;

                        // Add section title if the node is in subcategory
                        if (!titlePaths.Contains(fullTitleAsPath))
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(title))
                            {
                                level = level
                            });
                            titlePaths.Add(fullTitleAsPath);
                        }
                    }
                }

                tree.Add(AddEdgeNodeSearchEntry(nodeMenuItem, nodeName, level));
            }
        }

        protected virtual string GetEdgeNodePath(Dictionary<string, Type> nodePaths, NodeProvider.PortDescription nodeMenuItem)=>
             nodePaths.FirstOrDefault(kp => kp.Value == nodeMenuItem.nodeType).Key;
        
        protected virtual SearchTreeEntry AddEdgeNodeSearchEntry(NodeProvider.PortDescription nodeMenuItem, string nodeName, int level)
        {
            return new SearchTreeEntry(new GUIContent($"{nodeName}:  {nodeMenuItem.portDisplayName}", icon))
            {
                level = level + 1,
                userData = nodeMenuItem
            };
        }

        // Node creation when validate a choice
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            // window to graph position
            var windowRoot = window.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - window.position.position);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            if (searchTreeEntry.userData is Type nodeType)
            {
                graphView.RegisterCompleteObjectUndo("Added " + nodeType);
                var view = graphView.AddNode(CreateNode(nodeType, graphMousePosition));
            }
            else if (searchTreeEntry.userData is NodeProvider.PortDescription desc)
            {
                nodeType = (desc).nodeType;
                graphView.RegisterCompleteObjectUndo("Added " + nodeType);
                PortView targetPort = CreateEdgePort(graphMousePosition, desc);
                if (inputPortView == null)
                    graphView.Connect(targetPort, outputPortView);
                else
                    graphView.Connect(inputPortView, targetPort);
            }
            else if (searchTreeEntry.userData is string menuItem)
            {
               graphView.AddNode( CreateCustomNode(menuItem, graphMousePosition));
            }
            return true;
        }

        protected virtual PortView CreateEdgePort(Vector2 graphMousePosition, NodeProvider.PortDescription desc)
        {
            var view =  graphView.AddNode(BaseNode.CreateFromType(desc.nodeType, graphMousePosition) );
            return view.GetPortViewFromFieldName(desc.portFieldName, desc.portIdentifier);
        }

        protected virtual BaseNode CreateNode(Type nodeType, Vector2 graphMousePosition) => BaseNode.CreateFromType(nodeType, graphMousePosition);
        protected virtual BaseNode CreateCustomNode(string menuItem, Vector2 graphMousePosition) { return null; }
    }
}