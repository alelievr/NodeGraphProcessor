namespace Cr7Sund.ConvertGraph
{
    using UnityEngine;
    using GraphProcessor;
    using UnityEditor;

    public class ConvertGraphWindow : BaseGraphWindow
    {
        public string GUID = string.Empty;
        ConvertGraph tmpGraph;

        // Add the window in the editor menu
        [MenuItem("CR7Sund/01_DefaultGraph #g")]
        public static void Open()
        {
            var graphWindow = GetWindow<ConvertGraphWindow>();

            graphWindow.tmpGraph = ScriptableObject.CreateInstance<ConvertGraph>();
            ProjectWindowUtil.CreateAsset(graphWindow.tmpGraph, "ConvertGraph.asset");

            graphWindow.InitializeGraph(graphWindow.tmpGraph);
            graphWindow.Show();
        }

        protected override void InitializeWindow(BaseGraph graph)
        {
            if (graphView == null)
            {
                graphView = new ConvertGraphView(this);
                graphView.Add(new ConvertGraphToolbarView(graphView));
            }

            rootView.Add(graphView);

            ConvertCollector.NothingToDo();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

    }
}