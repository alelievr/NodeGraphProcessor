namespace Cr7Sund.ConvertGraph
{
    using System;
    using System.Linq;
    using GraphProcessor;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class ConvertGraphView : BaseGraphView
    {
        CreateConvertNodeMenuWindow createCovnertNodeMenu;
        public ConvertGraphView(EditorWindow window) : base(window)
        {
            createCovnertNodeMenu = ScriptableObject.CreateInstance<CreateConvertNodeMenuWindow>();
            createCovnertNodeMenu.Initialize(this, window);
        }
        protected override void InitSearchWindow()
        {
            nodeCreationRequest = (c) => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), createCovnertNodeMenu);
        }

        protected override BaseEdgeConnectorListener CreateEdgeConnectorListener() => new ConvertEdgeConnectorListener(this);
      
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendSeparator();

            foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries())
            {
                var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                Vector2 nodePosition = mousePos;
                string actionName = "Create/" + (nodeMenuItem.Value == typeof(ConvertNode) ?  nodeMenuItem.Key.Split('$')[0] : nodeMenuItem.Key);
                evt.menu.AppendAction(actionName,
                    (e) =>
                    {
                        if (nodeMenuItem.Value == typeof(ConvertNode)) CreateConverterNodeOfClass(nodeMenuItem.Key, nodePosition);
                        else CreateNodeOfType(nodeMenuItem.Value, nodePosition);
                    }, DropdownMenuAction.AlwaysEnabled
                );

            }

            base.BuildContextualMenu(evt);
        }

        private void CreateConverterNodeOfClass(string convertClass, Vector2 position)
        {
            RegisterCompleteObjectUndo("Added " + convertClass + " node");
            var node = ConvertNode.CreateFromType(typeof(ConvertNode), position) as ConvertNode;
            string[] vs = convertClass.Split('$');
            node.typeInfo.typeName = vs[0].Split('/').LastOrDefault();
            node.typeInfo.assemblyName = vs[1];
            AddNode(node);
        }

        void CreateNodeOfType(Type type, Vector2 position)
        {
            RegisterCompleteObjectUndo("Added " + type + " node");
            AddNode(BaseNode.CreateFromType(type, position));
        }
    }
}