using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GraphProcessor
{
    public delegate IEnumerable<PortData> CustomPortBehaviorDelegate(List<SerializableEdge> edges);
    public delegate IEnumerable<PortData> CustomPortTypeBehaviorDelegate(string fieldName, string displayName, object value);

    [Serializable]
    public abstract class BaseNode
    {
        #region Nested type: NodeFieldInformation
        internal class NodeFieldInformation
        {
            #region Fields
            public string name;
            public string fieldName;
            public FieldInfo info;
            public bool input;
            public bool isMultiple;
            public string tooltip;
            public CustomPortBehaviorDelegate behavior;
            public bool vertical;
            #endregion

            #region Constructors
            public NodeFieldInformation(
                FieldInfo info,
                string name,
                bool input,
                bool isMultiple,
                string tooltip,
                bool vertical,
                CustomPortBehaviorDelegate behavior)
            {
                this.input = input;
                this.isMultiple = isMultiple;
                this.info = info;
                this.name = name;
                fieldName = info.Name;
                this.behavior = behavior;
                this.tooltip = tooltip;
                this.vertical = vertical;
            }
            #endregion
        }
        #endregion

        #region Nested type: PortUpdate
        private struct PortUpdate
        {
            public List<string> fieldNames;
            public BaseNode node;

            public void Deconstruct(out List<string> fieldNames, out BaseNode node)
            {
                fieldNames = this.fieldNames;
                node = this.node;
            }
        }
        #endregion

        #region Nested type: ProcessDelegate
        public delegate void ProcessDelegate();
        #endregion

        #region Fields
        [SerializeField]
        internal string nodeCustomName  ; // The name of the node in case it was renamed by a user

        //id
        public string GUID;

        public int computeOrder = -1;

        /// <summary>
        /// Container of input ports
        /// </summary>
        [NonSerialized]
        public readonly NodeInputPortContainer inputPorts;
        /// <summary>
        /// Container of output ports
        /// </summary>
        [NonSerialized]
        public readonly NodeOutputPortContainer outputPorts;

        //Node view datas
        public Rect position;
        /// <summary>
        /// Is the node expanded
        /// </summary>
        public bool expanded;
        /// <summary>
        /// Is debug visible
        /// </summary>
        public bool debug;
        /// <summary>
        /// Node locked state
        /// </summary>
        public bool                 nodeLock;

        [NonSerialized] private bool _needsInspector  ;

        [NonSerialized]
        internal Dictionary<string, NodeFieldInformation> nodeFields = new();

        [NonSerialized]
        internal Dictionary<Type, CustomPortTypeBehaviorDelegate> customPortTypeBehaviorMap = new();

        [NonSerialized] private List<string> messages = new();

        [NonSerialized]
        protected BaseGraph graph;

        // Used in port update algorithm
        private Stack<PortUpdate> fieldsToUpdate = new();
        private HashSet<PortUpdate> updatedFields = new();
        #endregion

        #region Properties
        /// <summary>
        /// Name of the node, it will be displayed in the title section
        /// </summary>
        /// <returns></returns>
        public virtual string       name => GetType().Name;

        /// <summary>
        /// The accent color of the node
        /// </summary>
        public virtual Color color => Color.clear;

        /// <summary>
        /// Set a custom uss file for the node. We use a Resources.Load to get the stylesheet so be sure to put the correct
        /// resources path
        /// https://docs.unity3d.com/ScriptReference/Resources.Load.html
        /// </summary>
        public virtual string       layoutStyle => string.Empty;

        /// <summary>
        /// If the node can be locked or not
        /// </summary>
        public virtual bool         unlockable => true;

        /// <summary>
        /// Is the node is locked (if locked it can't be moved)
        /// </summary>
        public virtual bool         isLocked => nodeLock;

        /// <summary>
        /// Tell wether or not the node can be processed. Do not check anything from inputs because this step happens
        /// before inputs are sent to the node
        /// </summary>
        public virtual bool canProcess => true;

        /// <summary>Show the node controlContainer only when the mouse is over the node</summary>
        public virtual bool showControlsOnHover => false;

        /// <summary>True if the node can be deleted, false otherwise</summary>
        public virtual bool deletable => true;

        /// <summary>
        /// Does the node needs to be visible in the inspector (when selected).
        /// </summary>
        public virtual bool needsInspector => _needsInspector;

        /// <summary>
        /// Can the node be renamed in the UI. By default a node can be renamed by double clicking it's name.
        /// </summary>
        public virtual bool isRenamable => false;

        /// <summary>
        /// Is the node created from a duplicate operation (either ctrl-D or copy/paste).
        /// </summary>
        public bool createdFromDuplication { get; internal set; } = false;

        /// <summary>
        /// True only when the node was created from a duplicate operation and is inside a group that was also duplicated at the
        /// same time.
        /// </summary>
        public bool createdWithinGroup { get; internal set; } = false;
        #endregion

        #region Events
        public void OnEdgeConnected(SerializableEdge edge)
        {
            var input = edge._inputNode == this;
            NodePortContainer portCollection = input ? inputPorts : outputPorts;

            portCollection.Add(edge);

            UpdateAllPorts();

            onAfterEdgeConnected?.Invoke(edge);
        }

        public void OnEdgeDisconnected(SerializableEdge edge)
        {
            if (edge == null)
            {
                return;
            }

            var input = edge._inputNode == this;
            NodePortContainer portCollection = input ? inputPorts : outputPorts;

            portCollection.Remove(edge);

            // Reset default values of input port:
            var haveConnectedEdges = edge._inputNode.inputPorts.Where(p => p.fieldName == edge.inputFieldName).Any(p => p.GetEdges().Count != 0);
            if (edge._inputNode == this && !haveConnectedEdges && CanResetPort(edge._inputPort))
            {
                edge._inputPort?.ResetToDefault();
            }

            UpdateAllPorts();

            onAfterEdgeDisconnected?.Invoke(edge);
        }

        public void OnProcess()
        {
            inputPorts.PullDatas();

            ExceptionToLog.Call(() => Process());

            InvokeOnProcessed();

            outputPorts.PushDatas();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Triggered when the node is processes
        /// </summary>
        public event ProcessDelegate onProcessed;
        public event Action<string, NodeMessageType> onMessageAdded;
        public event Action<string> onMessageRemoved;
        /// <summary>
        /// Triggered after an edge was connected on the node
        /// </summary>
        public event Action<SerializableEdge> onAfterEdgeConnected;
        /// <summary>
        /// Triggered after an edge was disconnected on the node
        /// </summary>
        public event Action<SerializableEdge> onAfterEdgeDisconnected;

        /// <summary>
        /// Triggered after a single/list of port(s) is updated, the parameter is the field name
        /// </summary>
        public event Action<string> onPortsUpdated;

        /// <summary>
        /// Creates a node of type T at a certain position
        /// </summary>
        /// <param name="position">position in the graph in pixels</param>
        /// <typeparam name="T">type of the node</typeparam>
        /// <returns>the node instance</returns>
        public static T CreateFromType<T>(Vector2 position) where T : BaseNode
        {
            return CreateFromType(typeof(T), position) as T;
        }

        /// <summary>
        /// Creates a node of type nodeType at a certain position
        /// </summary>
        /// <param name="position">position in the graph in pixels</param>
        /// <typeparam name="nodeType">type of the node</typeparam>
        /// <returns>the node instance</returns>
        public static BaseNode CreateFromType(Type nodeType, Vector2 position)
        {
            if (!nodeType.IsSubclassOf(typeof(BaseNode)))
            {
                return null;
            }

            var node = Activator.CreateInstance(nodeType) as BaseNode;

            node.position = new Rect(position, new Vector2(100, 100));

            ExceptionToLog.Call(() => node.OnNodeCreated());

            return node;
        }

        protected virtual bool CanResetPort(NodePort port)
        {
            return true;
        }

        public void InvokeOnProcessed()
        {
            onProcessed?.Invoke();
        }

        /// <summary>
        /// Called when the node is enabled
        /// </summary>
        protected virtual void Enable() { }

        /// <summary>
        /// Called when the node is disabled
        /// </summary>
        protected virtual void Disable() { }

        /// <summary>
        /// Called when the node is removed
        /// </summary>
        protected virtual void Destroy() { }

        /// <summary>
        /// Override this method to implement custom processing
        /// </summary>
        protected virtual void Process() { }
        #endregion

        #region Initialization
        // called by the BaseGraph when the node is added to the graph
        public void Initialize(BaseGraph graph)
        {
            this.graph = graph;

            ExceptionToLog.Call(() => Enable());

            InitializePorts();
        }

        private void InitializeCustomPortTypeMethods()
        {
            var methods = new MethodInfo[0];
            var baseType = GetType();
            while (true)
            {
                methods = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    var typeBehaviors = method.GetCustomAttributes<CustomPortTypeBehavior>().ToArray();

                    if (typeBehaviors.Length == 0)
                    {
                        continue;
                    }

                    CustomPortTypeBehaviorDelegate deleg = null;
                    try
                    {
                        deleg = Delegate.CreateDelegate(typeof(CustomPortTypeBehaviorDelegate), this, method) as CustomPortTypeBehaviorDelegate;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        Debug.LogError($"Cannot convert method {method} to a delegate of type {typeof(CustomPortTypeBehaviorDelegate)}");
                    }

                    foreach (var typeBehavior in typeBehaviors)
                    {
                        customPortTypeBehaviorMap[typeBehavior.type] = deleg;
                    }
                }

                // Try to also find private methods in the base class
                baseType = baseType.BaseType;
                if (baseType == null)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Use this function to initialize anything related to ports generation in your node
        /// This will allow the node creation menu to correctly recognize ports that can be connected between nodes
        /// </summary>
        public virtual void InitializePorts()
        {
            InitializeCustomPortTypeMethods();

            foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))
            {
                var nodeField = nodeFields[key.Name];

                if (HasCustomBehavior(nodeField))
                {
                    UpdatePortsForField(nodeField.fieldName, false);
                }
                else
                {
                    // If we don't have a custom behavior on the node, we just have to create a simple port
                    AddPort(nodeField.input, nodeField.fieldName, new PortData { acceptMultipleEdges = nodeField.isMultiple, displayName = nodeField.name, tooltip = nodeField.tooltip, vertical = nodeField.vertical });
                }
            }
        }

        /// <summary>
        /// Override the field order inside the node. It allows to re-order all the ports and field in the UI.
        /// </summary>
        /// <param name="fields">List of fields to sort</param>
        /// <returns>Sorted list of fields</returns>
        public virtual IEnumerable<FieldInfo> OverrideFieldOrder(IEnumerable<FieldInfo> fields)
        {
            long GetFieldInheritanceLevel(FieldInfo f)
            {
                var level = 0;
                var t = f.DeclaringType;
                while (t != null)
                {
                    t = t.BaseType;
                    level++;
                }

                return level;
            }

            // Order by MetadataToken and inheritance level to sync the order with the port order (make sure FieldDrawers are next to the correct port)
            return fields.OrderByDescending(f => (GetFieldInheritanceLevel(f) << 32) | (uint)f.MetadataToken);
        }

        protected BaseNode()
        {
            inputPorts = new NodeInputPortContainer(this);
            outputPorts = new NodeOutputPortContainer(this);

            InitializeInOutDatas();
        }

        /// <summary>
        /// Update all ports of the node
        /// </summary>
        public bool UpdateAllPorts()
        {
            var changed = false;

            foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))
            {
                var field = nodeFields[key.Name];
                changed |= UpdatePortsForField(field.fieldName);
            }

            return changed;
        }

        /// <summary>
        /// Update all ports of the node without updating the connected ports. Only use this method when you need to update all the
        /// nodes ports in your graph.
        /// </summary>
        public bool UpdateAllPortsLocal()
        {
            var changed = false;

            foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))
            {
                var field = nodeFields[key.Name];
                changed |= UpdatePortsForFieldLocal(field.fieldName);
            }

            return changed;
        }

        /// <summary>
        /// Update the ports related to one C# property field (only for this node)
        /// </summary>
        /// <param name="fieldName"></param>
        public bool UpdatePortsForFieldLocal(string fieldName, bool sendPortUpdatedEvent = true)
        {
            var changed = false;

            if (!nodeFields.ContainsKey(fieldName))
            {
                return false;
            }

            var fieldInfo = nodeFields[fieldName];

            if (!HasCustomBehavior(fieldInfo))
            {
                return false;
            }

            var finalPorts = new List<string>();

            var portCollection = fieldInfo.input ? (NodePortContainer)inputPorts : outputPorts;

            // Gather all fields for this port (before to modify them)
            var nodePorts = portCollection.Where(p => p.fieldName == fieldName).ToList();
            // Gather all edges connected to these fields:
            var edges = nodePorts.SelectMany(n => n.GetEdges()).ToList();

            if (fieldInfo.behavior != null)
            {
                foreach (var portData in fieldInfo.behavior(edges))
                {
                    AddPortData(portData);
                }
            }
            else
            {
                var customPortTypeBehavior = customPortTypeBehaviorMap[fieldInfo.info.FieldType];

                foreach (var portData in customPortTypeBehavior(fieldName, fieldInfo.name, fieldInfo.info.GetValue(this)))
                {
                    AddPortData(portData);
                }
            }

            void AddPortData(PortData portData)
            {
                var port = nodePorts.FirstOrDefault(n => n.portData.identifier == portData.identifier);
                // Guard using the port identifier so we don't duplicate identifiers
                if (port == null)
                {
                    AddPort(fieldInfo.input, fieldName, portData);
                    changed = true;
                }
                else
                {
                    // in case the port type have changed for an incompatible type, we disconnect all the edges attached to this port
                    if (!BaseGraph.TypesAreConnectable(port.portData.displayType, portData.displayType))
                    {
                        foreach (var edge in port.GetEdges().ToList())
                        {
                            graph.Disconnect(edge.GUID);
                        }
                    }

                    // patch the port data
                    if (port.portData != portData)
                    {
                        port.portData.CopyFrom(portData);
                        changed = true;
                    }
                }

                finalPorts.Add(portData.identifier);
            }

            // TODO
            // Remove only the ports that are no more in the list

            foreach (var currentPort in nodePorts
                         .Where(
                             currentPort => finalPorts
                                 .All(id => id != currentPort.portData.identifier)))
            {
                RemovePort(fieldInfo.input, currentPort);
                changed = true;
            }

            // Make sure the port order is correct:
            portCollection.Sort(
                (p1, p2) =>
                {
                    var p1Index = finalPorts.FindIndex(id => p1.portData.identifier == id);
                    var p2Index = finalPorts.FindIndex(id => p2.portData.identifier == id);

                    if (p1Index == -1 || p2Index == -1)
                    {
                        return 0;
                    }

                    return p1Index.CompareTo(p2Index);
                });

            if (sendPortUpdatedEvent)
            {
                onPortsUpdated?.Invoke(fieldName);
            }

            return changed;
        }

        private bool HasCustomBehavior(NodeFieldInformation info)
        {
            if (info.behavior != null)
            {
                return true;
            }

            if (customPortTypeBehaviorMap.ContainsKey(info.info.FieldType))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update the ports related to one C# property field and all connected nodes in the graph
        /// </summary>
        /// <param name="fieldName"></param>
        public bool UpdatePortsForField(string fieldName, bool sendPortUpdatedEvent = true)
        {
            var changed  = false;

            fieldsToUpdate.Clear();
            updatedFields.Clear();

            fieldsToUpdate.Push(
                new PortUpdate
                {
                    fieldNames = new List<string>
                        { fieldName },
                    node = this
                });

            // Iterate through all the ports that needs to be updated, following graph connection when the 
            // port is updated. This is required ton have type propagation multiple nodes that changes port types
            // are connected to each other (i.e. the relay node)
            while (fieldsToUpdate.Count != 0)
            {
                var (fields, node) = fieldsToUpdate.Pop();

                // Avoid updating twice a port
                if (updatedFields.Any(t => t.node == node && fields.SequenceEqual(t.fieldNames)))
                {
                    continue;
                }
                updatedFields.Add(new PortUpdate { fieldNames = fields, node = node });

                foreach (var field in fields)
                {
                    if (node.UpdatePortsForFieldLocal(field, sendPortUpdatedEvent))
                    {
                        NodePortContainer portContainer = node.IsFieldInput(field) ? node.inputPorts : node.outputPorts;
                        foreach (var port in portContainer)
                        {
                            if (port.fieldName != field)
                            {
                                continue;
                            }

                            foreach (var edge in port.GetEdges())
                            {
                                var edgeNode = node.IsFieldInput(field) ? edge._outputNode : edge._inputNode;
                                var fieldsWithBehavior = edgeNode.nodeFields.Values
                                    .Where(HasCustomBehavior)
                                    .Select(f => f.fieldName).ToList();
                                fieldsToUpdate.Push(new PortUpdate { fieldNames = fieldsWithBehavior, node = edgeNode });
                            }
                        }
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private HashSet<BaseNode> portUpdateHashSet = new();

        internal void DisableInternal()
        {
            // port containers are initialized in the OnEnable
            inputPorts.Clear();
            outputPorts.Clear();

            ExceptionToLog.Call(() => Disable());
        }

        internal void DestroyInternal()
        {
            ExceptionToLog.Call(() => Destroy());
        }

        /// <summary>
        /// Called only when the node is created, not when instantiated
        /// </summary>
        public virtual void OnNodeCreated()
        {
            GUID = Guid.NewGuid().ToString();
        }

        public virtual FieldInfo[] GetNodeFields()
        {
            return GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void InitializeInOutDatas()
        {
            var fields = GetNodeFields();
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var inputAttribute = field.GetCustomAttribute<InputAttribute>();
                var outputAttribute = field.GetCustomAttribute<OutputAttribute>();
                var tooltipAttribute = field.GetCustomAttribute<TooltipAttribute>();
                var showInInspector = field.GetCustomAttribute<ShowInInspector>();
                var vertical = field.GetCustomAttribute<VerticalAttribute>();
                var isMultiple = false;
                var input = false;
                var name = field.Name;
                string tooltip = null;

                if (showInInspector != null)
                {
                    _needsInspector = true;
                }

                if (inputAttribute == null && outputAttribute == null)
                {
                    continue;
                }

                //check if field is a collection type
                isMultiple = inputAttribute?.allowMultiple ?? outputAttribute.allowMultiple;
                input = inputAttribute != null;
                tooltip = tooltipAttribute?.tooltip;

                if (!string.IsNullOrEmpty(inputAttribute?.name))
                {
                    name = inputAttribute.name;
                }
                if (!string.IsNullOrEmpty(outputAttribute?.name))
                {
                    name = outputAttribute.name;
                }

                // By default we set the behavior to null, if the field have a custom behavior, it will be set in the loop just below
                nodeFields[field.Name] = new NodeFieldInformation(
                    field,
                    name,
                    input,
                    isMultiple,
                    tooltip,
                    vertical != null,
                    null);
            }

            foreach (var method in methods)
            {
                var customPortBehaviorAttribute = method.GetCustomAttribute<CustomPortBehaviorAttribute>();
                CustomPortBehaviorDelegate behavior = null;

                if (customPortBehaviorAttribute == null)
                {
                    continue;
                }

                // Check if custom port behavior function is valid
                try
                {
                    var referenceType = typeof(CustomPortBehaviorDelegate);
                    behavior = (CustomPortBehaviorDelegate)Delegate.CreateDelegate(
                        referenceType,
                        this,
                        method,
                        true);
                }
                catch
                {
                    Debug.LogError("The function " + method + " cannot be converted to the required delegate format: " + typeof(CustomPortBehaviorDelegate));
                }

                if (nodeFields.ContainsKey(customPortBehaviorAttribute.fieldName))
                {
                    nodeFields[customPortBehaviorAttribute.fieldName].behavior = behavior;
                }
                else
                {
                    Debug.LogError("Invalid field name for custom port behavior: " + method + ", " + customPortBehaviorAttribute.fieldName);
                }
            }
        }
        #endregion

        #region API and utils
        /// <summary>
        /// Add a port
        /// </summary>
        /// <param name="input">is input port</param>
        /// <param name="fieldName">C# field name</param>
        /// <param name="portData">Data of the port</param>
        public void AddPort(bool input, string fieldName, PortData portData)
        {
            // Fixup port data info if needed:
            if (portData.displayType == null)
            {
                portData.displayType = nodeFields[fieldName].info.FieldType;
            }

            if (input)
            {
                inputPorts.Add(new NodePort(this, fieldName, portData));
            }
            else
            {
                outputPorts.Add(new NodePort(this, fieldName, portData));
            }
        }

        /// <summary>
        /// Remove a port
        /// </summary>
        /// <param name="input">is input port</param>
        /// <param name="port">the port to delete</param>
        public void RemovePort(bool input, NodePort port)
        {
            if (input)
            {
                inputPorts.Remove(port);
            }
            else
            {
                outputPorts.Remove(port);
            }
        }

        /// <summary>
        /// Remove port(s) from field name
        /// </summary>
        /// <param name="input">is input</param>
        /// <param name="fieldName">C# field name</param>
        public void RemovePort(bool input, string fieldName)
        {
            if (input)
            {
                inputPorts.RemoveAll(p => p.fieldName == fieldName);
            }
            else
            {
                outputPorts.RemoveAll(p => p.fieldName == fieldName);
            }
        }

        /// <summary>
        /// Get all the nodes connected to the input ports of this node
        /// </summary>
        /// <returns>an enumerable of node</returns>
        public IEnumerable<BaseNode> GetInputNodes()
        {
            foreach (var port in inputPorts)
            foreach (var edge in port.GetEdges())
            {
                yield return edge._outputNode;
            }
        }

        /// <summary>
        /// Get all the nodes connected to the output ports of this node
        /// </summary>
        /// <returns>an enumerable of node</returns>
        public IEnumerable<BaseNode> GetOutputNodes()
        {
            foreach (var port in outputPorts)
            foreach (var edge in port.GetEdges())
            {
                yield return edge._inputNode;
            }
        }

        /// <summary>
        /// Return a node matching the condition in the dependencies of the node
        /// </summary>
        /// <param name="condition">Condition to choose the node</param>
        /// <returns>Matched node or null</returns>
        public BaseNode FindInDependencies(Func<BaseNode, bool> condition)
        {
            var dependencies = new Stack<BaseNode>();

            dependencies.Push(this);

            var depth = 0;
            while (dependencies.Count > 0)
            {
                var node = dependencies.Pop();

                // Guard for infinite loop (faster than a HashSet based solution)
                depth++;
                if (depth > 2000)
                {
                    break;
                }

                if (condition(node))
                {
                    return node;
                }

                foreach (var dep in node.GetInputNodes())
                {
                    dependencies.Push(dep);
                }
            }
            return null;
        }

        /// <summary>
        /// Get the port from field name and identifier
        /// </summary>
        /// <param name="fieldName">C# field name</param>
        /// <param name="identifier">Unique port identifier</param>
        /// <returns></returns>
        public NodePort GetPort(string fieldName, string identifier)
        {
            return inputPorts.Concat(outputPorts).FirstOrDefault(
                p =>
                {
                    var bothNull = string.IsNullOrEmpty(identifier) && string.IsNullOrEmpty(p.portData.identifier);
                    return p.fieldName == fieldName && (bothNull || identifier == p.portData.identifier);
                });
        }

        /// <summary>
        /// Return all the ports of the node
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NodePort> GetAllPorts()
        {
            foreach (var port in inputPorts)
            {
                yield return port;
            }
            foreach (var port in outputPorts)
            {
                yield return port;
            }
        }

        /// <summary>
        /// Return all the connected edges of the node
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SerializableEdge> GetAllEdges()
        {
            foreach (var port in GetAllPorts())
            foreach (var edge in port.GetEdges())
            {
                yield return edge;
            }
        }

        /// <summary>
        /// Is the port an input
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public bool IsFieldInput(string fieldName)
        {
            return nodeFields[fieldName].input;
        }

        /// <summary>
        /// Add a message on the node
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        public void AddMessage(string message, NodeMessageType messageType)
        {
            if (messages.Contains(message))
            {
                return;
            }

            onMessageAdded?.Invoke(message, messageType);
            messages.Add(message);
        }

        /// <summary>
        /// Remove a message on the node
        /// </summary>
        /// <param name="message"></param>
        public void RemoveMessage(string message)
        {
            onMessageRemoved?.Invoke(message);
            messages.Remove(message);
        }

        /// <summary>
        /// Remove a message that contains
        /// </summary>
        /// <param name="subMessage"></param>
        public void RemoveMessageContains(string subMessage)
        {
            var toRemove = messages.Find(m => m.Contains(subMessage));
            messages.Remove(toRemove);
            onMessageRemoved?.Invoke(toRemove);
        }

        /// <summary>
        /// Remove all messages on the node
        /// </summary>
        public void ClearMessages()
        {
            foreach (var message in messages)
            {
                onMessageRemoved?.Invoke(message);
            }
            messages.Clear();
        }

        /// <summary>
        /// Set the custom name of the node. This is intended to be used by renamable nodes.
        /// This custom name will be serialized inside the node.
        /// </summary>
        /// <param name="customName">New name of the node.</param>
        public void SetCustomName(string customName)
        {
            nodeCustomName = customName;
        }

        /// <summary>
        /// Get the name of the node. If the node have a custom name (set using the UI by double clicking on the node title) then
        /// it will return this name first, otherwise it returns the value of the name field.
        /// </summary>
        /// <returns>The name of the node as written in the title</returns>
        public string GetCustomName()
        {
            return string.IsNullOrEmpty(nodeCustomName) ? name : nodeCustomName;
        }
        #endregion
    }
}