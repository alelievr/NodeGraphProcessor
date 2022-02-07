using System.Reflection;
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
    class CreateNodeMenuWindow : ScriptableObject, ISearchWindowProvider
    {
        BaseGraphView graphView;
        EditorWindow window;
        Texture2D icon;
        EdgeView edgeFilter;
        PortView inputPortView;
        PortView outputPortView;

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

            if (edgeFilter == null)
                CreateStandardNodeMenu(tree);
            else
                CreateEdgeNodeMenu(tree);

            return tree;
        }

        void CreateStandardNodeMenu(List<SearchTreeEntry> tree)
        {
            // Sort menu by alphabetical order and submenus
            var nodeEntries = graphView.FilterCreateNodeMenuEntries()
                .Concat(graphView.FilterCreateCustomNodeMenuEntries())
                .OrderBy(k => k.path);

            var titlePaths = new HashSet<string>();

            foreach (var nodeMenuItem in nodeEntries)
            {
                var nodePath = nodeMenuItem.path;
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

                tree.Add(new SearchTreeEntry(new GUIContent(nodeName, icon))
                {
                    level = level + 1,
                    userData = new Tuple<Type, Func<Type, Vector2, BaseNode>>(nodeMenuItem.type, nodeMenuItem.creationMethod)
                });
            }
        }

        void CreateEdgeNodeMenu(List<SearchTreeEntry> tree)
        {
            var entries = NodeProvider.GetEdgeCreationNodeMenuEntry((edgeFilter.input ?? edgeFilter.output) as PortView, graphView.graph);

            var titlePaths = new HashSet<string>();

            var nodePaths = NodeProvider.GetNodeMenuEntries(graphView.graph).Concat(NodeProvider.GetCustomNodeMenuEntries(graphView.graph));
            // var customMenuEntries = 

            tree.Add(new SearchTreeEntry(new GUIContent($"Relay", icon))
            {
                level = 1,
                userData = new NodeProvider.PortDescription
                {
                    nodeType = typeof(RelayNode),
                    portType = typeof(System.Object),
                    isInput = inputPortView != null,
                    portFieldName = inputPortView != null ? nameof(RelayNode.output) : nameof(RelayNode.input),
                    portIdentifier = "0",
                    portDisplayName = inputPortView != null ? "Out" : "In",
                }
            });

            var sortedMenuItems = entries.Select(port => (port, nodePaths.FirstOrDefault(kp => kp.type == port.nodeType).path)).OrderBy(e => e.path);

            // Sort menu by alphabetical order and submenus
            foreach (var nodeMenuItem in sortedMenuItems)
            {
                foreach (var node in nodePaths.Where(kp => kp.type == nodeMenuItem.port.nodeType))
                {
                    var nodePath = node.path;

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

                    tree.Add(new SearchTreeEntry(new GUIContent($"{nodeName}:  {nodeMenuItem.port.portDisplayName}", icon))
                    {
                        level = level + 1,
                        userData = new Tuple<NodeProvider.PortDescription, Func<Type, Vector2, BaseNode>>(nodeMenuItem.port, node.creationMethod)
                    });
                }
            }
        }

        // Node creation when validate a choice
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            // window to graph position
            var windowRoot = window.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - window.position.position);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            if (searchTreeEntry.userData is Tuple<Type, Func<Type, Vector2, BaseNode>>)
            {
                Tuple<Type, Func<Type, Vector2, BaseNode>> userData = searchTreeEntry.userData as Tuple<Type, Func<Type, Vector2, BaseNode>>;
                var nodeType = userData.Item1;
                var method = userData.Item2;

                graphView.RegisterCompleteObjectUndo("Added " + nodeType);
                graphView.AddNode(method.Invoke(nodeType, graphMousePosition));
            }
            else
            {
                Tuple<NodeProvider.PortDescription, Func<Type, Vector2, BaseNode>> userData = searchTreeEntry.userData as Tuple<NodeProvider.PortDescription, Func<Type, Vector2, BaseNode>>;
                var nodeType = userData.Item1.nodeType;
                var method = userData.Item2;

                graphView.RegisterCompleteObjectUndo("Added " + nodeType);
                BaseNodeView view = graphView.AddNode(method.Invoke(nodeType, graphMousePosition));

                var targetPort = view.GetPortViewFromFieldName(userData.Item1.portFieldName, userData.Item1.portIdentifier);
                if (inputPortView == null)
                    graphView.Connect(targetPort, outputPortView);
                else
                    graphView.Connect(inputPortView, targetPort);
            }

            return true;
        }
    }
}