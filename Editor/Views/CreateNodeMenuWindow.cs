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
        BaseGraphView   graphView;
        EditorWindow    window;
        Texture2D       icon;

        public void Initialize(BaseGraphView graphView, EditorWindow window)
        {
            this.graphView = graphView;
            this.window = window;

            // Transparent icon to trick search window into indenting items
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

            var nodeEntries = graphView.FilterCreateNodeMenuEntries().OrderBy(k => k.Key);
            var titles = new HashSet< string >();

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
                    for(var i = 0; i < parts.Length - 1; i++)
                    {
                        var title = parts[i];
                        level = i + 1;
                        // Add section title if the node is in subcategory
                        if (!titles.Contains(title))
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(title)){
                                level = level
                            });
                            titles.Add(title);
                        }
                    }
                }
                
                tree.Add(new SearchTreeEntry(new GUIContent(nodeName, icon))
                {
                    level    = level + 1,
                    userData = nodeMenuItem.Value
                });
			}

            return tree;
        }

        // Node creation when validate a choice
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            // window to graph position
            var windowRoot = window.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - window.position.position);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            graphView.RegisterCompleteObjectUndo("Added " + searchTreeEntry.userData);
            graphView.AddNode(BaseNode.CreateFromType((Type)searchTreeEntry.userData, graphMousePosition));
            return true;
        }
    }
}