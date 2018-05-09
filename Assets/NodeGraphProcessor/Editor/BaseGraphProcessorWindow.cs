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
	public abstract class BaseGraphProcessorWindow : EditorWindow
	{
		protected VisualElement		rootView;
		protected BaseGraphView		graphView;
	
		private void OnEnable()
		{
			InitializeRootView();

			InitializeGraphView();
		}

		void InitializeRootView()
		{
			rootView = this.GetRootVisualContainer();
	
			rootView.AddStyleSheetPath("Styles/Graph");
		}

		void InitializeGraphView()
		{
			graphView = new BaseGraphView();
	
			graphView.AddManipulator(new ContentDragger());
			graphView.AddManipulator(new SelectionDragger());
			graphView.AddManipulator(new RectangleSelector());
			graphView.AddManipulator(new ClickSelector());
			
			graphView.SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);
	
			graphView.StretchToParentSize();
			
			rootView.Add(graphView);
		}

		public void Initialize(BaseGraph graph)
		{
			graphView.Initialize(graph);
		}
	}
}