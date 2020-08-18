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
        protected override SearchTreeEntry AddSearchEntry(KeyValuePair<string, Type> nodeMenuItem, string nodeName, int level)
        {
            if (nodeMenuItem.Value == typeof(ConvertNode))
            {
                string[] vs = nodeMenuItem.Key.Split('$');
                var typeName = vs[0].Split('/').LastOrDefault();

                var treeEntry = base.AddSearchEntry(nodeMenuItem, nodeName, level);
                treeEntry.userData = nodeMenuItem.Key;
                treeEntry.content.text = typeName;

                return treeEntry;
            }
            else
            {
                return base.AddSearchEntry(nodeMenuItem, nodeName, level);
            }
        }

        protected override BaseNode CreateCustomNode(string menuItem, Vector2 graphMousePosition)
        {
            var node = BaseNode.CreateFromType(typeof(ConvertNode), graphMousePosition) as ConvertNode;
            string[] vs =  menuItem.Split('$');

            node.typeInfo = new TypeInfo()
            {
                typeName = vs[0].Split('/').LastOrDefault(),
                assemblyName = vs[1]
            };

            return node;
        }
    }
}