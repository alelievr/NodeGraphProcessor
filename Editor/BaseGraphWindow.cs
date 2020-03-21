using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace GraphProcessor
{
	[System.Serializable]
	public abstract class BaseGraphWindow : EditorWindow
	{
		protected VisualElement		rootView;
		protected BaseGraphView		graphView;

		[SerializeField]
		protected BaseGraph			graph;

		readonly string				graphWindowStyle = "GraphProcessorStyles/BaseGraphView";

		public bool					isGraphLoaded
		{
			get { return graphView != null && graphView.graph != null; }
		}

		public event Action< BaseGraph >	graphLoaded;
		public event Action< BaseGraph >	graphUnloaded;

		/// <summary>
		/// Called by Unity when the window is enabled / opened
		/// </summary>
		protected virtual void OnEnable()
		{
			InitializeRootView();

			if (graph != null)
			{
				// We wait for the graph to be initialized
				if (graph.isEnabled)
					InitializeGraph(graph);
				else
					graph.onEnabled += () => InitializeGraph(graph);
			}
		}

		/// <summary>
		/// Called by Unity when the window is disabled (happens on domain reload)
		/// </summary>
		protected virtual void OnDisable()
		{
			if (graph != null && graphView != null)
				graphView.SaveGraphToDisk();
		}
		
		/// <summary>
		/// Called by Unity when the window is closed
		/// </summary>
		protected virtual void OnDestroy() { }

		void InitializeRootView()
		{
			rootView = base.rootVisualElement;

			rootView.name = "graphRootView";

			rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
		}

		public void InitializeGraph(BaseGraph graph)
		{
			if (this.graph != null && graph != this.graph)
			{
				// Save the graph to the disk
				EditorUtility.SetDirty(this.graph);
				AssetDatabase.SaveAssets();
				// Unload the graph
				graphUnloaded?.Invoke(this.graph);
				Resources.UnloadAsset(this.graph);
			}

			graphLoaded?.Invoke(graph);
			this.graph = graph;

			if (graphView != null)
				rootView.Remove(graphView);

			//Initialize will provide the BaseGraphView
			InitializeWindow(graph);

			graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;

			if (graphView == null)
			{
				Debug.LogError("GraphView has not been added to the BaseGraph root view !");
				return ;
			}

			graphView.Initialize(graph);

			InitializeGraphView(graphView);
		}

		public virtual void OnGraphDeleted()
		{
			if (graph != null)
				rootView.Remove(graphView);

			graphView = null;
		}

		protected abstract void	InitializeWindow(BaseGraph graph);
		protected virtual void InitializeGraphView(BaseGraphView view) {}
	}
}