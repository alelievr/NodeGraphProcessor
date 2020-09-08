namespace Cr7Sund.ConvertGraph
{
    using System.Collections.Generic;
    using GraphProcessor;
    using UnityEditor;
    using UnityEngine;

    public class SubGraphWindow : BaseGraphWindow
    {
        public string serialData;
        public List<string> oldGUIDs;
        public SubGraphNode subGraphNode;

        protected override void InitializeWindow(BaseGraph graph)
        {
            if (graphView == null)
            {
                graphView = new SubGraphView(this);
            }
            rootView.Add(graphView);
        }

        protected override void InitializeGraphView(BaseGraphView view)
        {
            if (graph.nodes.Count > 0) return;
            var subGraphView = graphView as SubGraphView;
            subGraphView.PasteGraph(serialData, oldGUIDs,subGraphNode);
        }
    }
}