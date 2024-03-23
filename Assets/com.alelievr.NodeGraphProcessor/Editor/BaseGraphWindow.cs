using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    [Serializable]
    public abstract class BaseGraphWindow : EditorWindow
    {
        #region Fields
        protected VisualElement rootView;
        protected BaseGraphView graphView;

        [SerializeField]
        protected BaseGraph graph;

        private readonly string graphWindowStyle = "GraphProcessorStyles/BaseGraphView";

        private bool reloadWorkaround;
        #endregion

        #region Properties
        public bool isGraphLoaded => graphView != null && graphView.graph != null;
        #endregion

        #region Events
        /// <summary>
        /// Called by Unity when the window is closed
        /// </summary>
        protected virtual void OnDestroy() { }

        public virtual void OnGraphDeleted()
        {
            if (graph != null && graphView != null)
            {
                rootView.Remove(graphView);
            }

            graphView = null;
        }
        #endregion

        #region Methods
        public event Action<BaseGraph> graphLoaded;
        public event Action<BaseGraph> graphUnloaded;

        /// <summary>
        /// Called by Unity when the window is enabled / opened
        /// </summary>
        protected virtual void OnEnable()
        {
            minSize = new Vector2(400, 400);
            InitializeRootView();

            if (graph != null)
            {
                LoadGraph();
            }
            else
            {
                reloadWorkaround = true;
            }
        }

        protected virtual void Update()
        {
            // Workaround for the Refresh option of the editor window:
            // When Refresh is clicked, OnEnable is called before the serialized data in the
            // editor window is deserialized, causing the graph view to not be loaded
            if (reloadWorkaround && graph != null)
            {
                LoadGraph();
                reloadWorkaround = false;
            }
        }

        private void LoadGraph()
        {
            // We wait for the graph to be initialized
            if (graph.isEnabled)
            {
                InitializeGraph(graph);
            }
            else
            {
                graph.onEnabled += () => InitializeGraph(graph);
            }
        }

        /// <summary>
        /// Called by Unity when the window is disabled (happens on domain reload)
        /// </summary>
        protected virtual void OnDisable()
        {
            if (graph != null && graphView != null)
            {
                graphView.SaveGraphToDisk();
            }
        }

        private void InitializeRootView()
        {
            rootView = rootVisualElement;

            rootView.name = "graphRootView";

            var stylesheet = Resources.Load<StyleSheet>(graphWindowStyle);
            if (stylesheet == null)
            {
                throw new Exception("GraphProcessor: can't find the stylesheet for the graph window: " + graphWindowStyle);
            }

            rootView.styleSheets.Add(stylesheet);
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
            }

            graphLoaded?.Invoke(graph);
            this.graph = graph;

            if (graphView != null)
            {
                graphView.ClearGraphElements();
                rootView.Remove(graphView);
            }

            //Initialize will provide the BaseGraphView
            InitializeWindow(graph);

            graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;

            if (graphView == null)
            {
                Debug.LogError("GraphView has not been added to the BaseGraph root view !");
                return;
            }

            graphView.Initialize(graph);

            InitializeGraphView(graphView);

            // TOOD: onSceneLinked...

            if (graph.IsLinkedToScene())
            {
                LinkGraphWindowToScene(graph.GetLinkedScene());
            }
            else
            {
                graph.onSceneLinked += LinkGraphWindowToScene;
            }
        }

        private void LinkGraphWindowToScene(Scene scene)
        {
            EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;

            void CloseWindowWhenSceneIsClosed(Scene closedScene)
            {
                if (scene == closedScene)
                {
                    Close();
                    EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
                }
            }
        }

        protected abstract void InitializeWindow(BaseGraph graph);
        protected virtual void InitializeGraphView(BaseGraphView view) { }
        #endregion
    }
}