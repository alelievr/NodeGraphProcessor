namespace Cr7Sund.ConvertGraph
{
    using System.Collections.Generic;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using GraphProcessor;
    using UnityEditor;
    using System.Linq;
    using UnityEngine.UIElements;
    using System;
    using System.CodeDom;

    public class CreateConvertNodeMenuWindow : CreateNodeMenuWindow
    {
        protected override SearchTreeEntry AddStandardSearchEntry(KeyValuePair<string, Type> nodeMenuItem, string nodeName, int level)
        {
            if (nodeMenuItem.Value == typeof(ConvertNode))
            {
                string[] vs = nodeMenuItem.Key.Split('$');
                var typeName = vs[0].Split('/').LastOrDefault();

                var treeEntry = base.AddStandardSearchEntry(nodeMenuItem, nodeName, level);
                treeEntry.userData = nodeMenuItem.Key;
                treeEntry.content.text = typeName;

                return treeEntry;
            }
            else
            {
                return base.AddStandardSearchEntry(nodeMenuItem, nodeName, level);
            }
        }

        protected override SearchTreeEntry AddEdgeNodeSearchEntry(NodeProvider.PortDescription nodeMenuItem, string nodeName, int level)
        {

            if (nodeMenuItem.nodeType.IsAssignableFrom(typeof(ConvertNode)))
            {

                string[] vs = nodeMenuItem.extraInfo.Split('$');
                return new SearchTreeEntry(new GUIContent($"{vs[0].Split('/').LastOrDefault()}:  {nodeMenuItem.portDisplayName}", icon))
                {
                    level = level + 1,
                    userData = nodeMenuItem
                };
            }
            else
            {

                return base.AddEdgeNodeSearchEntry(nodeMenuItem, nodeName, level);
            }
        }

        protected override PortView CreateEdgePort(Vector2 graphMousePosition, NodeProvider.PortDescription desc)
        {
            if (desc.nodeType.IsAssignableFrom(typeof(ConvertNode)))
            {
                string[] vs = desc.extraInfo.Split('$');
                var node = BaseNode.CreateFromType(typeof(ConvertNode), graphMousePosition) as ConvertNode;

                node.typeInfo = new TypeInfo()
                {
                    typeName = vs[0].Split('/').LastOrDefault(),
                    assemblyName = vs[1]
                };
                var view = graphView.AddNode(node) as ConvertNodeView;
                if (view.PortFileKeys.Length != 2 && view.PortFileKeys[0] != "inputs")
                {
                    throw new Exception("You are not modifying a convert node or \n you have change the order about inputs and outputs");
                }

                return view.GetPortViewFromFieldName(
                    desc.isInput ? view.PortFileKeys[0] : view.PortFileKeys[1],
                    desc.portIdentifier
                );
            }
            else
            {
                return base.CreateEdgePort(graphMousePosition, desc);
            }
        }

        protected override BaseNode CreateCustomNode(string menuItem, Vector2 graphMousePosition)
        {
            var node = BaseNode.CreateFromType(typeof(ConvertNode), graphMousePosition) as ConvertNode;
            string[] vs = menuItem.Split('$');

            node.typeInfo = new TypeInfo()
            {
                typeName = vs[0].Split('/').LastOrDefault(),
                assemblyName = vs[1]
            };

            graphView.RegisterCompleteObjectUndo("Added " + node.typeInfo.typeName);
            return node;
        }
    }
}