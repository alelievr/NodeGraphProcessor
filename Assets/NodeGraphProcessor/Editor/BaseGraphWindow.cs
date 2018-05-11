using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using Unity.Properties;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;

using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing;

namespace GraphProcessor
{
	[System.Serializable]
	public abstract class BaseGraphWindow : EditorWindow
	{
		protected VisualElement		rootView;
		protected BaseGraphView		graphView;

		[SerializeField]
		protected BaseGraph			graph;

		public bool					isGraphLoaded
		{
			get { return graphView != null && graphView.graph != null; }
		}
	
		protected void OnEnable()
		{
			InitializeRootView();

			if (graph != null)
				InitializeGraph(graph);
		}

		void InitializeRootView()
		{
			rootView = this.GetRootVisualContainer();

			rootView.name = "graphRootView";
	
			rootView.AddStyleSheetPath("Styles/BaseGraphView");
		}

		public void InitializeGraph(BaseGraph graph)
		{
			this.graph = graph;

			//Initialize will provide the BaseGraphView
			Initialize(graph);

			graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;

			if (graphView == null)
			{
				Debug.LogError("GraphView has not been added to the BaseGraph root view !");
				return ;
			}

			graphView.Initialize(graph);
		}

		bool first = true;

		protected abstract void	Initialize(BaseGraph graph);
	}
}