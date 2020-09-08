namespace Cr7Sund.ConvertGraph
{
    using UnityEngine;
    using GraphProcessor;
    using UnityEditor;
    using UnityEngine.UIElements;
    using System.Collections.Generic;
    using System;
    using UnityEditor.UIElements;

    public class ConvertGraphWindow : BaseGraphWindow
    {
        public string guid;
        public bool isM2V;
        public ConvertGraph convertGraph;

        public static Dictionary<BaseGraph, ConvertGraphWindow> convertGraphMap = new Dictionary<BaseGraph, ConvertGraphWindow>();
        public ConvertGraphToolbarView toolBarView;

        protected override void InitializeWindow(BaseGraph graph)
        {
            if (graph is ConvertGraph cGraph)
            {
                convertGraph = cGraph;
            }

            if (graphView == null)
            {
                graphView = new ConvertGraphView(this);
                ((ConvertGraphView)graphView).isM2V = isM2V;

                toolBarView = new ConvertGraphToolbarView(graphView);
                graphView.Add(toolBarView);
            }

            rootView.Add(graphView);

            ConvertCollector.CheckEverythingIsValid();
        }

        public void AddSourceNodeAtFirst(Type dataType, bool isM2V, Vector2 pos, string nodeName, bool isDynamic)
        {
            var cGraphView = graphView as ConvertGraphView;
            cGraphView.AddSourceNode(dataType, isM2V, pos, nodeName, isDynamic);
        }

        protected override void OnDisable()
        {
            convertGraphMap.Remove(graph);
            if (graphView == null) return;
            ((ConvertGraphView)graphView).Save();
        }

        [MenuItem("Window/MVVM/Close ConvertGraphWindow _%k")]
        private static void CloseWindow()
        {
            if (EditorWindow.HasOpenInstances<ConvertGraphWindow>())
            {
                EditorUtility.SetDirty(GetWindow<ConvertGraphWindow>().graph);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                GetWindow<ConvertGraphWindow>().Close();
            }
        }

        void ShowButton(Rect position)
        {
            var _helpButtonStyle = new GUIStyle("IconButton");

            var iconContent = EditorGUIUtility.IconContent("_Help");
            if (GUI.Button(position, iconContent, _helpButtonStyle))
            {
                Application.OpenURL(convertGraph.helpUrl);
            }
        }
    }
}