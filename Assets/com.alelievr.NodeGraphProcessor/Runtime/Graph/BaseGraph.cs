using System;
using System.Collections.Generic;
using System.Linq;
using cpGames.core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace GraphProcessor
{
    public class GraphChanges
    {
        #region Fields
        public SerializableEdge removedEdge;
        public SerializableEdge addedEdge;
        public BaseNode removedNode;
        public BaseNode addedNode;
        public BaseNode nodeChanged;
        public Group addedGroups;
        public Group removedGroups;
        public BaseStackNode addedStackNode;
        public BaseStackNode removedStackNode;
        public StickyNote addedStickyNotes;
        public StickyNote removedStickyNotes;
        #endregion
    }

    /// <summary>
    /// Compute order type used to determine the compute order integer on the nodes
    /// </summary>
    public enum ComputeOrderType
    {
        DepthFirst,
        BreadthFirst
    }

    [Serializable]
    public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver
    {
        #region Fields
        private static readonly int maxComputeOrderDepth = 1000;

        /// <summary>Invalid compute order number of a node when it's inside a loop</summary>
        public static readonly int loopComputeOrder = -2;
        /// <summary>Invalid compute order number of a node can't process</summary>
        public static readonly int invalidComputeOrder = -1;

        /// <summary>
        /// Json list of serialized nodes only used for copy pasting in the editor. Note that this field isn't serialized
        /// </summary>
        /// <typeparam name="JsonElement"></typeparam>
        /// <returns></returns>
        [SerializeField] [Obsolete("Use BaseGraph.nodes instead")]
        public List<JsonElement> serializedNodes = new();

        /// <summary>
        /// List of all the nodes in the graph.
        /// </summary>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [SerializeReference]
        public List<BaseNode> nodes = new();

        /// <summary>
        /// Dictionary to access node per GUID, faster than a search in a list
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [NonSerialized]
        public Dictionary<string, BaseNode> nodesPerGUID = new();

        /// <summary>
        /// Json list of edges
        /// </summary>
        /// <typeparam name="SerializableEdge"></typeparam>
        /// <returns></returns>
        [SerializeField]
        public List<SerializableEdge> edges = new();
        /// <summary>
        /// Dictionary of edges per GUID, faster than a search in a list
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="SerializableEdge"></typeparam>
        /// <returns></returns>
        [NonSerialized]
        public Dictionary<string, SerializableEdge> edgesPerGUID = new();

        /// <summary>
        /// All groups in the graph
        /// </summary>
        /// <typeparam name="Group"></typeparam>
        /// <returns></returns>
        [SerializeField] [FormerlySerializedAs("commentBlocks")]
        public List<Group>                     		groups = new();

        /// <summary>
        /// All Stack Nodes in the graph
        /// </summary>
        /// <typeparam name="stackNodes"></typeparam>
        /// <returns></returns>
        [SerializeField] [SerializeReference] // Polymorphic serialization
        public List<BaseStackNode> stackNodes = new();

        /// <summary>
        /// All pinned elements in the graph
        /// </summary>
        /// <typeparam name="PinnedElement"></typeparam>
        /// <returns></returns>
        [SerializeField]
        public List<PinnedElement> pinnedElements = new();

        /// <summary>
        /// All exposed parameters in the graph
        /// </summary>
        /// <typeparam name="ExposedParameter"></typeparam>
        /// <returns></returns>
        [SerializeField] [SerializeReference]
        public List<ExposedParameter> exposedParameters = new();

        [SerializeField] [FormerlySerializedAs("exposedParameters")] // We keep this for upgrade
        private List<ExposedParameter> serializedParameterList = new();

        [SerializeField]
        public List<StickyNote> stickyNotes = new();

        [NonSerialized] private Dictionary<BaseNode, int> computeOrderDictionary = new();

        [NonSerialized] private Scene linkedScene;

        // Trick to keep the node inspector alive during the editor session
        [SerializeField]
        internal Object nodeInspectorReference;

        //graph visual properties
        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;

        [NonSerialized] private bool _isEnabled  ;

        private HashSet<BaseNode> infiniteLoopTracker = new();
        #endregion

        #region Properties
        public bool isEnabled { get => _isEnabled; private set => _isEnabled = value; }

        public HashSet<BaseNode> graphOutputs { get; private set; } = new();
        #endregion

        #region Events
        public virtual void OnAssetDeleted() { }
        #endregion

        #region ISerializationCallbackReceiver Members
        public void OnBeforeSerialize()
        {
            // Cleanup broken elements
            stackNodes.RemoveAll(s => s == null);
            nodes.RemoveAll(n => n == null);
        }

        public void OnAfterDeserialize() { }
        #endregion

        #region Methods
        /// <summary>
        /// Triggered when something is changed in the list of exposed parameters
        /// </summary>
        public event Action onExposedParameterListChanged;
        public event Action<ExposedParameter> onExposedParameterModified;
        public event Action<string, string> onExposedParameterRenamed;
        public event Action<ExposedParameter> onExposedParameterValueChanged;

        /// <summary>
        /// Triggered when the graph is linked to an active scene.
        /// </summary>
        public event Action<Scene> onSceneLinked;

        /// <summary>
        /// Triggered when the graph is enabled
        /// </summary>
        public event Action onEnabled;

        /// <summary>
        /// Triggered when the graph is changed
        /// </summary>
        public event Action<GraphChanges> onGraphChanges;

        protected virtual void OnEnable()
        {
            if (isEnabled)
            {
                OnDisable();
            }

            MigrateGraphIfNeeded();
            InitializeGraphElements();
            DestroyBrokenGraphElements();
            UpdateComputeOrder();
            isEnabled = true;
            onEnabled?.Invoke();
        }

        private void InitializeGraphElements()
        {
            // Sanitize the element lists (it's possible that nodes are null if their full class name have changed)
            // If you rename / change the assembly of a node or parameter, please use the MovedFrom() attribute to avoid breaking the graph.
            nodes.RemoveAll(n => n == null);
            exposedParameters.RemoveAll(e => e == null);

            foreach (var node in nodes.ToList())
            {
                nodesPerGUID[node.GUID] = node;
                node.Initialize(this);
            }

            foreach (var edge in edges.ToList())
            {
                edge.Deserialize(this);
                edgesPerGUID[edge.GUID] = edge;

                // Sanity check for the edge:
                if (edge._inputPort == null || edge._outputPort == null)
                {
                    Disconnect(edge.GUID);
                    continue;
                }

                // Add the edge to the non-serialized port data
                edge._inputPort.owner.OnEdgeConnected(edge);
                edge._outputPort.owner.OnEdgeConnected(edge);
            }
        }

        protected virtual void OnDisable()
        {
            isEnabled = false;
            foreach (var node in nodes)
            {
                node.DisableInternal();
            }
        }

        /// <summary>
        /// Adds a node to the graph
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public BaseNode AddNode(BaseNode node)
        {
            nodesPerGUID[node.GUID] = node;

            nodes.Add(node);
            node.Initialize(this);

            onGraphChanges?.Invoke(new GraphChanges { addedNode = node });

            return node;
        }

        /// <summary>
        /// Removes a node from the graph
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(BaseNode node)
        {
            node.DisableInternal();
            node.DestroyInternal();

            nodesPerGUID.Remove(node.GUID);

            nodes.Remove(node);

            onGraphChanges?.Invoke(new GraphChanges { removedNode = node });
        }

        /// <summary>
        /// Connect two ports with an edge
        /// </summary>
        /// <param name="inputPort">input port</param>
        /// <param name="outputPort">output port</param>
        /// <param name="DisconnectInputs">is the edge allowed to disconnect another edge</param>
        /// <returns>the connecting edge</returns>
        public SerializableEdge Connect(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true)
        {
            var edge = SerializableEdge.CreateNewEdge(this, inputPort, outputPort);

            //If the input port does not support multi-connection, we remove them
            if (autoDisconnectInputs && !inputPort.portData.acceptMultipleEdges)
            {
                foreach (var e in inputPort.GetEdges().ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    Disconnect(e);
                }
            }
            // same for the output port:
            if (autoDisconnectInputs && !outputPort.portData.acceptMultipleEdges)
            {
                foreach (var e in outputPort.GetEdges().ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    Disconnect(e);
                }
            }

            edges.Add(edge);

            // Add the edge to the list of connected edges in the nodes
            inputPort.owner.OnEdgeConnected(edge);
            outputPort.owner.OnEdgeConnected(edge);

            onGraphChanges?.Invoke(new GraphChanges { addedEdge = edge });

            return edge;
        }

        /// <summary>
        /// Disconnect two ports
        /// </summary>
        /// <param name="inputNode">input node</param>
        /// <param name="inputFieldName">input field name</param>
        /// <param name="outputNode">output node</param>
        /// <param name="outputFieldName">output field name</param>
        public void Disconnect(
            BaseNode inputNode,
            string inputFieldName,
            BaseNode outputNode,
            string outputFieldName)
        {
            edges.RemoveAll(
                r =>
                {
                    var remove = r._inputNode == inputNode
                        && r._outputNode == outputNode
                        && r.outputFieldName == outputFieldName
                        && r.inputFieldName == inputFieldName;

                    if (remove)
                    {
                        r._inputNode?.OnEdgeDisconnected(r);
                        r._outputNode?.OnEdgeDisconnected(r);
                        onGraphChanges?.Invoke(new GraphChanges { removedEdge = r });
                    }

                    return remove;
                });
        }

        /// <summary>
        /// Disconnect an edge
        /// </summary>
        /// <param name="edge"></param>
        public void Disconnect(SerializableEdge edge)
        {
            Disconnect(edge.GUID);
        }

        /// <summary>
        /// Disconnect an edge
        /// </summary>
        /// <param name="edgeGUID"></param>
        public void Disconnect(string edgeGUID)
        {
            var disconnectEvents = new List<(BaseNode, SerializableEdge)>();

            edges.RemoveAll(
                r =>
                {
                    if (r.GUID == edgeGUID)
                    {
                        disconnectEvents.Add((r._inputNode, r));
                        disconnectEvents.Add((r._outputNode, r));
                        onGraphChanges?.Invoke(new GraphChanges { removedEdge = r });
                    }
                    return r.GUID == edgeGUID;
                });

            // Delay the edge disconnect event to avoid recursion
            foreach (var (node, edge) in disconnectEvents)
            {
                node?.OnEdgeDisconnected(edge);
            }
        }

        /// <summary>
        /// Add a group
        /// </summary>
        /// <param name="block"></param>
        public void AddGroup(Group block)
        {
            groups.Add(block);
            onGraphChanges?.Invoke(new GraphChanges { addedGroups = block });
        }

        /// <summary>
        /// Removes a group
        /// </summary>
        /// <param name="block"></param>
        public void RemoveGroup(Group block)
        {
            groups.Remove(block);
            onGraphChanges?.Invoke(new GraphChanges { removedGroups = block });
        }

        /// <summary>
        /// Add a StackNode
        /// </summary>
        /// <param name="stackNode"></param>
        public void AddStackNode(BaseStackNode stackNode)
        {
            stackNodes.Add(stackNode);
            onGraphChanges?.Invoke(new GraphChanges { addedStackNode = stackNode });
        }

        /// <summary>
        /// Remove a StackNode
        /// </summary>
        /// <param name="stackNode"></param>
        public void RemoveStackNode(BaseStackNode stackNode)
        {
            stackNodes.Remove(stackNode);
            onGraphChanges?.Invoke(new GraphChanges { removedStackNode = stackNode });
        }

        /// <summary>
        /// Add a sticky note
        /// </summary>
        /// <param name="note"></param>
        public void AddStickyNote(StickyNote note)
        {
            stickyNotes.Add(note);
            onGraphChanges?.Invoke(new GraphChanges { addedStickyNotes = note });
        }

        /// <summary>
        /// Removes a sticky note
        /// </summary>
        /// <param name="note"></param>
        public void RemoveStickyNote(StickyNote note)
        {
            stickyNotes.Remove(note);
            onGraphChanges?.Invoke(new GraphChanges { removedStickyNotes = note });
        }

        /// <summary>
        /// Invoke the onGraphChanges event, can be used as trigger to execute the graph when the content of a node is changed
        /// </summary>
        /// <param name="node"></param>
        public void NotifyNodeChanged(BaseNode node)
        {
            onGraphChanges?.Invoke(new GraphChanges { nodeChanged = node });
        }

        /// <summary>
        /// Open a pinned element of type viewType
        /// </summary>
        /// <param name="viewType">type of the pinned element</param>
        /// <returns>the pinned element</returns>
        public PinnedElement OpenPinned(Type viewType, Type modelType)
        {
            var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

            if (pinned == null)
            {
                pinned = Activator.CreateInstance(modelType, viewType) as PinnedElement;
                pinned.position = new Rect(pinned.DefaultPosition, pinned.DefaultSize);
                pinnedElements.Add(pinned);
            }
            else
            {
                pinned.opened = true;
            }

            return pinned;
        }

        /// <summary>
        /// Closes a pinned element of type viewType
        /// </summary>
        /// <param name="viewType">type of the pinned element</param>
        public void ClosePinned(Type viewType)
        {
            var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

            pinned.opened = false;
        }

        // We can deserialize data here because it's called in a unity context
        // so we can load objects references
        public void Deserialize()
        {
            // Disable nodes correctly before removing them:
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.DisableInternal();
                }
            }

            MigrateGraphIfNeeded();

            InitializeGraphElements();
        }

        public void MigrateGraphIfNeeded()
        {
#pragma warning disable CS0618
            // Migration step from JSON serialized nodes to [SerializeReference]
            if (serializedNodes.Count > 0)
            {
                nodes.Clear();
                foreach (var node in serializedNodes.ToList().Select(JsonSerializer.DeserializeNode).Where(node => node != null))
                {
                    nodes.Add(node);
                }
                serializedNodes.Clear();

                // we also migrate parameters here:
                var paramsToMigrate = serializedParameterList.ToList();
                exposedParameters.Clear();
                foreach (var param in paramsToMigrate)
                {
                    if (param == null)
                    {
                        continue;
                    }

                    var newParam = param.Migrate();

                    if (newParam == null)
                    {
                        Debug.LogError($"Can't migrate parameter of type {param.type}, please create an Exposed Parameter class that implements this type.");
                        continue;
                    }
                    exposedParameters.Add(newParam);
                }
            }
#pragma warning restore CS0618
        }

        /// <summary>
        /// Update the compute order of the nodes in the graph
        /// </summary>
        /// <param name="type">Compute order type</param>
        public void UpdateComputeOrder(ComputeOrderType type = ComputeOrderType.DepthFirst)
        {
            if (nodes.Count == 0)
            {
                return;
            }

            // Find graph outputs (end nodes) and reset compute order
            graphOutputs.Clear();
            foreach (var node in nodes)
            {
                if (node.GetOutputNodes().Count() == 0)
                {
                    graphOutputs.Add(node);
                }
                node.computeOrder = 0;
            }

            computeOrderDictionary.Clear();
            infiniteLoopTracker.Clear();

            switch (type)
            {
                default:
                case ComputeOrderType.DepthFirst:
                    UpdateComputeOrderDepthFirst();
                    break;
                case ComputeOrderType.BreadthFirst:
                    foreach (var node in nodes)
                    {
                        UpdateComputeOrderBreadthFirst(0, node);
                    }
                    break;
            }
        }

        /// <summary>
        /// Add an exposed parameter
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="type">parameter type (must be a subclass of ExposedParameter)</param>
        /// <param name="value">default value</param>
        /// <returns>The unique id of the parameter</returns>
        public string AddExposedParameter(string name, Type type, object value = null)
        {
            if (!type.IsSubclassOf(typeof(ExposedParameter)))
            {
                Debug.LogError($"Can't add parameter of type {type}, the type doesn't inherit from ExposedParameter.");
            }

            var param = Activator.CreateInstance(type) as ExposedParameter;

            // patch value with correct type:
            if (param.ValueType.IsValueType)
            {
                value = Activator.CreateInstance(param.ValueType);
            }

            param.Initialize(name, value);
            exposedParameters.Add(param);

            onExposedParameterListChanged?.Invoke();

            return param.guid;
        }

        /// <summary>
        /// Add an already allocated / initialized parameter to the graph
        /// </summary>
        /// <param name="parameter">The parameter to add</param>
        /// <returns>The unique id of the parameter</returns>
        public string AddExposedParameter(ExposedParameter parameter)
        {
            var guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter

            parameter.guid = guid;
            exposedParameters.Add(parameter);

            onExposedParameterListChanged?.Invoke();

            return guid;
        }

        /// <summary>
        /// Remove an exposed parameter
        /// </summary>
        /// <param name="ep">the parameter to remove</param>
        public void RemoveExposedParameter(ExposedParameter ep)
        {
            exposedParameters.Remove(ep);

            onExposedParameterListChanged?.Invoke();
        }

        /// <summary>
        /// Remove an exposed parameter
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        public void RemoveExposedParameter(string guid)
        {
            if (exposedParameters.RemoveAll(e => e.guid == guid) != 0)
            {
                onExposedParameterListChanged?.Invoke();
            }
        }

        internal void NotifyExposedParameterListChanged()
        {
            onExposedParameterListChanged?.Invoke();
        }

        /// <summary>
        /// Update an exposed parameter value
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <param name="value">new value</param>
        public void UpdateExposedParameter(string guid, object value)
        {
            var param = exposedParameters.Find(e => e.guid == guid);
            if (param == null)
            {
                return;
            }

            if (value != null && !param.ValueType.IsInstanceOfType(value))
            {
                throw new Exception("Type mismatch when updating parameter " + param.name + ": from " + param.ValueType + " to " + value.GetType().AssemblyQualifiedName);
            }

            param.Value = value;
            onExposedParameterModified?.Invoke(param);
        }

        /// <summary>
        /// Update the exposed parameter name
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="name">new name</param>
        public void UpdateExposedParameterName(ExposedParameter parameter, string name)
        {
            var oldname = parameter.name;
            if (oldname == name)
            {
                return;
            }
            parameter.name = name;
            onExposedParameterRenamed?.Invoke(oldname, name);
            onExposedParameterModified?.Invoke(parameter);
        }

        /// <summary>
        /// Update parameter visibility
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="isHidden">is Hidden</param>
        public void NotifyExposedParameterChanged(ExposedParameter parameter)
        {
            onExposedParameterModified?.Invoke(parameter);
        }

        public void NotifyExposedParameterValueChanged(ExposedParameter parameter)
        {
            onExposedParameterValueChanged?.Invoke(parameter);
        }

        /// <summary>
        /// Get the exposed parameter from name
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>the parameter or null</returns>
        public ExposedParameter GetExposedParameter(string name)
        {
            return exposedParameters.FirstOrDefault(e => e.name == name);
        }

        /// <summary>
        /// Get exposed parameter from GUID
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <returns>The parameter</returns>
        public ExposedParameter GetExposedParameterFromGUID(string guid)
        {
            return exposedParameters.FirstOrDefault(e => e?.guid == guid);
        }

        /// <summary>
        /// Set parameter value from name. (Warning: the parameter name can be changed by the user)
        /// </summary>
        /// <param name="name">name of the parameter</param>
        /// <param name="value">new value</param>
        /// <returns>true if the value have been assigned</returns>
        public bool SetParameterValue(string name, object value)
        {
            var e = exposedParameters.FirstOrDefault(p => p.name == name);

            if (e == null)
            {
                return false;
            }

            e.Value = value;

            return true;
        }

        /// <summary>
        /// Get the parameter value
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <returns>value</returns>
        public object GetParameterValue(string name)
        {
            return exposedParameters.FirstOrDefault(p => p.name == name)?.Value;
        }

        /// <summary>
        /// Get the parameter value template
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <typeparam name="T">type of the parameter</typeparam>
        /// <returns>value</returns>
        public T GetParameterValue<T>(string name)
        {
            return (T)GetParameterValue(name);
        }

        /// <summary>
        /// Link the current graph to the scene in parameter, allowing the graph to pick and serialize objects from the scene.
        /// </summary>
        /// <param name="scene">Target scene to link</param>
        public void LinkToScene(Scene scene)
        {
            linkedScene = scene;
            onSceneLinked?.Invoke(scene);
        }

        /// <summary>
        /// Return true when the graph is linked to a scene, false otherwise.
        /// </summary>
        public bool IsLinkedToScene()
        {
            return linkedScene.IsValid();
        }

        /// <summary>
        /// Get the linked scene. If there is no linked scene, it returns an invalid scene
        /// </summary>
        public Scene GetLinkedScene()
        {
            return linkedScene;
        }

        private int UpdateComputeOrderBreadthFirst(int depth, BaseNode node)
        {
            var computeOrder = 0;

            if (depth > maxComputeOrderDepth)
            {
                Debug.LogError("Recursion error while updating compute order");
                return -1;
            }

            if (computeOrderDictionary.ContainsKey(node))
            {
                return node.computeOrder;
            }

            if (!infiniteLoopTracker.Add(node))
            {
                return -1;
            }

            if (!node.canProcess)
            {
                node.computeOrder = -1;
                computeOrderDictionary[node] = -1;
                return -1;
            }

            foreach (var dep in node.GetInputNodes())
            {
                var c = UpdateComputeOrderBreadthFirst(depth + 1, dep);

                if (c == -1)
                {
                    computeOrder = -1;
                    break;
                }

                computeOrder += c;
            }

            if (computeOrder != -1)
            {
                computeOrder++;
            }

            node.computeOrder = computeOrder;
            computeOrderDictionary[node] = computeOrder;

            return computeOrder;
        }

        private void UpdateComputeOrderDepthFirst()
        {
            var dfs = new Stack<BaseNode>();

            GraphUtils.FindCyclesInGraph(
                this,
                n =>
                {
                    PropagateComputeOrder(n, loopComputeOrder);
                });

            var computeOrder = 0;
            foreach (var node in GraphUtils.DepthFirstSort(this)
                         .Where(node => node.computeOrder != loopComputeOrder))
            {
                if (!node.canProcess)
                {
                    node.computeOrder = -1;
                }
                else
                {
                    node.computeOrder = computeOrder++;
                }
            }
        }

        private void PropagateComputeOrder(BaseNode node, int computeOrder)
        {
            var deps = new Stack<BaseNode>();
            var loop = new HashSet<BaseNode>();

            deps.Push(node);
            while (deps.Count > 0)
            {
                var n = deps.Pop();
                n.computeOrder = computeOrder;

                if (!loop.Add(n))
                {
                    continue;
                }

                foreach (var dep in n.GetOutputNodes())
                {
                    deps.Push(dep);
                }
            }
        }

        private void DestroyBrokenGraphElements()
        {
            edges.RemoveAll(
                e => e._inputNode == null
                    || e._outputNode == null
                    || string.IsNullOrEmpty(e.outputFieldName)
                    || string.IsNullOrEmpty(e.inputFieldName)
            );
            nodes.RemoveAll(n => n == null);
        }

        /// <summary>
        /// Tell if two types can be connected in the context of a graph
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static bool TypesAreConnectable(Type t1, Type t2)
        {
            if (t1 == null || t2 == null)
            {
                return false;
            }

            if (TypeAdapter.AreIncompatible(t1, t2))
            {
                return false;
            }

            //Check if there is custom adapters for this assignation
            if (CustomPortIO.IsAssignable(t1, t2))
            {
                return true;
            }

            //Check for type assignability
            if (t2.IsReallyAssignableFrom(t1))
            {
                return true;
            }

            // User defined type convertions
            if (TypeAdapter.AreAssignable(t1, t2))
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}