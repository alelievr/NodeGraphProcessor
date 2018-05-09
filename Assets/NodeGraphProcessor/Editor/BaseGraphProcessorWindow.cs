using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Properties;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;

using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing;

namespace GraphProcessor
{
	public abstract class BaseGraphWindow : EditorWindow
	{
		protected VisualElement		rootView;
		protected BaseGraphView		graphView;
	
		protected void OnEnable()
		{
			InitializeRootView();
		}

		void InitializeRootView()
		{
			rootView = this.GetRootVisualContainer();
	
			rootView.AddStyleSheetPath("Styles/Graph");
		}

		public void InitializeGraph(BaseGraph graph)
		{
			graphView.Initialize(graph);

			Initialize(graph);
		}

		protected virtual void	Initialize(BaseGraph graph) {}
	}
}