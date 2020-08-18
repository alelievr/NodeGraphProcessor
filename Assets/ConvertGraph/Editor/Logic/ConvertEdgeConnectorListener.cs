namespace Cr7Sund.ConvertGraph
{
    using UnityEngine;
    using GraphProcessor;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor;

    public class ConvertEdgeConnectorListener : BaseEdgeConnectorListener
    {
        public ConvertEdgeConnectorListener(BaseGraphView graphView) : base(graphView) { }

        protected override void ShowNodeCreationMenuFromEdge(EdgeView edgeView, Vector2 position)
        {
            if (edgeNodeCreateMenuWindow == null)
            {
                edgeNodeCreateMenuWindow = ScriptableObject.CreateInstance<CreateConvertNodeMenuWindow>();
            }

            edgeNodeCreateMenuWindow.Initialize(graphView, EditorWindow.focusedWindow, edgeView);
            SearchWindow.Open(new SearchWindowContext(position + EditorWindow.focusedWindow.position.position), edgeNodeCreateMenuWindow);
        }
    }
}