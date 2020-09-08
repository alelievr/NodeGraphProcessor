namespace Cr7Sund.ConvertGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphProcessor;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class ConvertGraphView : BaseGraphView
    {
        CreateConvertNodeMenuWindow createCovnertNodeMenu;
        ConvertGraphProcessor processor;
        public bool isM2V;

        public ConvertGraphView(EditorWindow window) : base(window)
        {
            createCovnertNodeMenu = ScriptableObject.CreateInstance<CreateConvertNodeMenuWindow>();
            createCovnertNodeMenu.Initialize(this, window);
        }
        protected override void InitSearchWindow()
        {
            nodeCreationRequest = (c) => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), createCovnertNodeMenu);
        }

        protected override BaseEdgeConnectorListener CreateEdgeConnectorListener() => new ConvertEdgeConnectorListener(this);

        public BaseNodeView AddSourceNode(Type dataType, bool isM2V, Vector2 pos, string nodeName, bool isDynamicType)
        {
            return this.AddNode(CreateSourceNode(dataType, isM2V, pos, GraphNodeType.MNode.ToString(), isDynamicType));
        }

        private BaseNode CreateSourceNode(Type dataType, bool isM2V, Vector2 pos, string nodeName, bool isDynamicType)
        {
            var cGraph = graph as ConvertGraph;
            var node = GraphNode.CreateFromType(typeof(SourceNode), pos) as SourceNode;
            node.graphNodeType = isM2V ? GraphNodeType.MNode : GraphNodeType.VNode;
            node.nodeName = nodeName;
            node.canChangeType = isDynamicType;
            if (isM2V)
            {
                node.mTypeInfo = new TypeInfo(dataType);
                cGraph.destNodeTypeInfo = node.mTypeInfo;
            }
            else
            {
                node.vTypeInfo = new TypeInfo(dataType);
                cGraph.startNodeTypeInfo = node.vTypeInfo;
            }
            return node;
        }

        private void CreateConverterNodeOfClass(string convertClass, Vector2 position)
        {
            RegisterCompleteObjectUndo($"Added {convertClass} node");
            var node = ConvertNode.CreateFromType(typeof(ConvertNode), position) as ConvertNode;

            string[] vs = convertClass.Split('$');

            node.classTypeInfo.assemblyName = vs[2];

            node.description = vs[1];
            string[] cFuncInfo = vs[0].Split('/');
            node.convertFuncName = cFuncInfo[cFuncInfo.Length - 1];
            node.classTypeInfo.fullName = cFuncInfo[cFuncInfo.Length - 2];

            AddNode(node);
        }

        void CreateNodeOfType(Type type, Vector2 position)
        {
            RegisterCompleteObjectUndo($"Added {type} node");
            AddNode(BaseNode.CreateFromType(type, position));
        }



        protected virtual void RemoveNodeWithEdges(GraphNodeView nodeView)
        {
            var inputPortViews = nodeView.inputPortViews;
            var outputPortViews = nodeView.outputPortViews;
            for (int i = 0; i < inputPortViews.Count; i++)
            {
                var edges = inputPortViews[i].GetEdges();
                for (int j = 0; j < edges.Count; j++)
                {
                    RemoveElement(edges[j]);
                }
            }
            for (int i = 0; i < outputPortViews.Count; i++)
            {
                var edges = outputPortViews[i].GetEdges();
                for (int j = 0; j < edges.Count; j++)
                {
                    RemoveElement(edges[j]);
                }
            }
            graph.RemoveNode(nodeView.nodeTarget);
            RemoveElement(nodeView);
        }

        private void RunProcessor()
        {
            processor = new ConvertGraphProcessor(this.graph);
            processor.Run();
        }


        #region  ContextualMenu
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendSeparator();

            BuildCreateNodeMenu(evt);
            BuildSubGraphMenu(evt);

            base.BuildContextualMenu(evt);
        }

        protected virtual void BuildSubGraphMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("SubGraph", (e) => AddSelectionsToSubGraph(position), DropdownMenuAction.AlwaysEnabled);
        }

        private void AddSelectionsToSubGraph(Vector2 position)
        {
            RegisterCompleteObjectUndo("SubGraph");

            var cGraph = graph as ConvertGraph;

            var newGraph = ScriptableObject.CreateInstance<SubGraph>();
            var newNode = BaseNode.CreateFromType(typeof(SubGraphNode), position) as SubGraphNode;

            var selectedNodeSet = new HashSet<string>();
            var selectedNodeViews = new List<GraphNodeView>();

            int inputIndex = 0, outputIndex = 0;
            var inputEdges = new List<Tuple<SerializableEdge, string>>();
            var outputEdges = new List<Tuple<SerializableEdge, string>>();
            var edgeMap = new Dictionary<string, List<SerializableEdge>>();

            for (int i = 0; i < selection.Count; i++)
            {
                ISelectable selectItem = selection[i];
                if (selectItem is ConvertNodeView graphNodeView)
                {
                    selectedNodeViews.Add(graphNodeView);
                    selectedNodeSet.Add(graphNodeView.nodeTarget.GUID);
                }
                else if (selectItem is SourceNodeView sourceNodeView)
                {
                    Debug.LogWarning("You can't subgraph your source node");
                    selection.Clear();
                    return;
                }
            }

            for (int i = 0; i < selectedNodeViews.Count; i++)
            {
                GraphNodeView selectItem = selectedNodeViews[i];
                var cNode = selectItem.nodeTarget as ConvertNode;

                edgeMap.Clear();
                var iEdges = cNode.GetInputEdges();
                foreach (var edge in iEdges)
                {
                    if (!edgeMap.ContainsKey(edge.outputNodeGUID))
                    {
                        edgeMap[edge.outputNodeGUID] = new List<SerializableEdge>();
                    }
                    edgeMap[edge.outputNodeGUID].Add(edge);
                }


                var inputNodes = cNode.GetInputNodes();
                foreach (var inputNode in inputNodes)
                {
                    if (!selectedNodeSet.Contains(inputNode.GUID))
                    {
                        var parameterInfos = cNode.ParameterInfos;
                        var index = inputIndex;
                        foreach (var parameterInfo in parameterInfos)
                        {
                            if (!parameterInfo.IsOut)
                            {
                                newNode.AddInputPorData(new SerialPortData
                                {
                                    displayName = parameterInfo.Name,
                                    displayType = new TypeInfo(parameterInfo.ParameterType),
                                    identifier = inputIndex.ToString(),
                                    guid = $"{inputNode.GUID}-{(inputIndex - index).ToString()}",
                                    position = new Vector2(cNode.position.x - (24 + 8) * 10, cNode.position.y + inputIndex * 24 * 10),
                                });
                                inputIndex++;
                            }
                        }

                        var list = edgeMap[inputNode.GUID];
                        for (int j = 0; j < list.Count; j++)
                        {
                            inputEdges.Add(new Tuple<SerializableEdge, string>(list[j], (Convert.ToInt32(list[j].inputPortIdentifier) + index).ToString()));
                        }

                        // PLAN Default Nodes (empty input)
                    }
                }


                edgeMap.Clear();
                var oEdges = cNode.GetOutputEdges();
                foreach (var edge in oEdges)
                {
                    if (!edgeMap.ContainsKey(edge.inputNodeGUID))
                    {
                        edgeMap[edge.inputNodeGUID] = new List<SerializableEdge>();
                    }
                    edgeMap[edge.inputNodeGUID].Add(edge);
                }

                var outputNodes = cNode.GetOutputNodes();
                foreach (var outputNode in outputNodes)
                {
                    if (!selectedNodeSet.Contains(outputNode.GUID))
                    {
                        int index = outputIndex;
                        var parameterInfos = cNode.ParameterInfos;
                        foreach (var parameterInfo in parameterInfos)
                        {
                            if (parameterInfo.IsOut)
                            {
                                newNode.AddOutputPorData(new SerialPortData
                                {
                                    displayName = parameterInfo.Name,
                                    displayType = new TypeInfo(parameterInfo.ParameterType.GetElementType()),
                                    identifier = outputIndex.ToString(),
                                    guid = $"{outputNode.GUID}-{(outputIndex - index).ToString()}",
                                    position = new Vector2(cNode.position.x + (24 + 8 * 10), cNode.position.y + outputIndex * 24 * 10),
                                });
                                outputIndex++;
                            }
                        }
                        var list = edgeMap[outputNode.GUID];
                        for (int j = 0; j < list.Count; j++)
                        {
                            outputEdges.Add(new Tuple<SerializableEdge, string>(list[j], (Convert.ToInt32(list[j].outputPortIdentifier) + index).ToString()));
                        }
                    }
                }

            }

            string path = EditorUtility.SaveFilePanel(
            "Save your convert graph",
            "",
            "NewConverterSubGraph",
            "asset"
            );

            if (string.IsNullOrEmpty(path)) return;

            path = $"Assets{path.Substring(Application.dataPath.Length)}";
            AssetDatabase.CreateAsset(newGraph, path);

            newNode.subGraphGUID = AssetDatabase.AssetPathToGUID(path);
            AddNode(newNode);

            var delayConnectEdgeViews = new List<EdgeView>();
            for (int i = 0; i < inputEdges.Count; i++)
            {
                var inputEdge = inputEdges[i].Item1;
                var oldOutputNode = inputEdge.outputNode;
                var outputPort = oldOutputNode.GetPort(inputEdge.outputPort.fieldName, inputEdge.outputPortIdentifier);
                var inputPort = newNode.GetPort(nameof(newNode.inputValue), inputEdges[i].Item2.ToString());
                var newEdge = SerializableEdge.CreateNewEdge(newGraph, inputPort, outputPort);

                if (nodeViewsPerNode.ContainsKey(oldOutputNode) && nodeViewsPerNode.ContainsKey(newNode))
                {
                    var edgeView = new EdgeView()
                    {
                        userData = newEdge,
                        input = nodeViewsPerNode[newNode].GetPortViewFromFieldName(newEdge.inputFieldName, newEdge.inputPortIdentifier),
                        output = nodeViewsPerNode[oldOutputNode].GetPortViewFromFieldName(newEdge.outputFieldName, newEdge.outputPortIdentifier)
                    };
                    Connect(edgeView);
                }
            }

            for (int i = 0; i < outputEdges.Count; i++)
            {
                var outputEdge = outputEdges[i].Item1;
                var oldInputNode = outputEdge.inputNode;
                var inputPort = oldInputNode.GetPort(outputEdge.inputPort.fieldName, outputEdge.inputPortIdentifier);
                var outputPort = newNode.GetPort(nameof(newNode.outputValue), outputEdges[i].Item2.ToString());
                var newEdge = SerializableEdge.CreateNewEdge(newGraph, inputPort, outputPort);

                if (nodeViewsPerNode.ContainsKey(oldInputNode) && nodeViewsPerNode.ContainsKey(newNode))
                {
                    var edgeView = new EdgeView()
                    {
                        userData = newEdge,
                        input = nodeViewsPerNode[oldInputNode].GetPortViewFromFieldName(newEdge.inputFieldName, newEdge.inputPortIdentifier),
                        output = nodeViewsPerNode[newNode].GetPortViewFromFieldName(newEdge.outputFieldName, newEdge.outputPortIdentifier)
                    };
                    delayConnectEdgeViews.Add(edgeView);
                }
            }

            string copyDatas = SerialzieSubGraphElements(selection.OfType<GraphElement>(), inputEdges.Select(v => v.Item1).Concat(outputEdges.Select(v => v.Item1)));

            for (int i = 0; i < selectedNodeViews.Count; i++)
            {
                RemoveNodeWithEdges(selectedNodeViews[i]);
            }

            // Reconnect
            for (int i = 0; i < delayConnectEdgeViews.Count; i++)
            {
                Connect(delayConnectEdgeViews[i]);
            }

            if (CanPasteSerializedDataCallback(copyDatas))
            {
                var subGraphWindow = EditorWindow.GetWindow<SubGraphWindow>();
                subGraphWindow.serialData = copyDatas;
                subGraphWindow.oldGUIDs = selectedNodeViews.Select(s => s.nodeTarget.GUID).ToList();
                subGraphWindow.subGraphNode = newNode;
                subGraphWindow.InitializeGraph(newGraph);
            }
        }

        public string SerialzieSubGraphElements(IEnumerable<GraphElement> elements, IEnumerable<SerializableEdge> allEdges)
        {
            var data = new CopyPasteHelper();

            foreach (BaseNodeView nodeView in elements.Where(e => e is BaseNodeView))
                data.copiedNodes.Add(JsonSerializer.SerializeNode(nodeView.nodeTarget));

            foreach (GroupView groupView in elements.Where(e => e is GroupView))
                data.copiedGroups.Add(JsonSerializer.Serialize(groupView.group));

            var edgeViewSet = new HashSet<string>();
            foreach (EdgeView edgeView in elements.Where(e => e is EdgeView))
            {
                data.copiedEdges.Add(JsonSerializer.Serialize(edgeView.serializedEdge));
                edgeViewSet.Add(edgeView.serializedEdge.GUID);
            }
            var edges = new List<SerializableEdge>(allEdges);
            foreach (var edge in allEdges)
            {
                if (!edgeViewSet.Contains(edge.GUID)) data.copiedEdges.Add(JsonSerializer.Serialize(edge));
            }

            ClearSelection();

            return JsonUtility.ToJson(data, true);
        }

        protected virtual void BuildCreateNodeMenu(ContextualMenuPopulateEvent evt)
        {
            foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries())
            {
                var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                Vector2 nodePosition = mousePos;
                string souceNodeName = GraphNodeType.MNode.ToString(); // Reverse direction
                string actionName = $"Create/{(nodeMenuItem.Value == typeof(ConvertNode) ? nodeMenuItem.Key.Split('$')[0].Split('.').LastOrDefault() : nodeMenuItem.Value == typeof(SourceNode) ? ($"{nodeMenuItem.Key}{souceNodeName}") : nodeMenuItem.Key)}";

                evt.menu.AppendAction(actionName,
                    (e) =>
                    {
                        if (nodeMenuItem.Value.IsAssignableFrom(typeof(ConvertNode))) CreateConverterNodeOfClass(nodeMenuItem.Key, nodePosition);
                        else if (nodeMenuItem.Value.IsAssignableFrom(typeof(SourceNode))) AddSourceNode(typeof(int), !isM2V, nodePosition, GraphNodeType.MNode.ToString(), true);
                        else CreateNodeOfType(nodeMenuItem.Value, nodePosition);
                    }, DropdownMenuAction.AlwaysEnabled
                );
            }
        }

        #endregion


        #region  Save


        public void Save()
        {
            if (graph is ConvertGraph convertGraph)
            {
                string path = AssetDatabase.GUIDToAssetPath(convertGraph.guid);

                if (string.IsNullOrEmpty(path))
                {
                    if (EditorUtility.DisplayDialog("Save Convert graph",
                    "Do you want to save your new created graph? ",     //\n Attention!!! The one you just created will be delete or override. For the god sake, put it into another folder
                    "Yes", "No, Thanks"))
                    {
                        SaveAsNew();
                        SaveWithProcessor();
                    }
                    else
                    {
                        convertGraph.guid = string.Empty;
                        if (EditorWindow.HasOpenInstances<ConvertGraphWindow>())
                        {
                            EditorWindow.GetWindow<ConvertGraphWindow>().Close();
                        }
                    }
                }
                else
                {
                    SaveWithProcessor();
                }
            }

        }

        private void SaveWithProcessor()
        {
            if (graph == null) return;
            // PLAN Compare type (Forbid if change type)
            Debug.LogWarning("Make sure you don't change the type both the MNode and VNode");
            RunProcessor();
            EditorApplication.delayCall += () =>
            {
                SaveGraphToDisk();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };
        }

        private void SaveAsNew()
        {
            if (graph is ConvertGraph convertGraph)
            {
                string path = EditorUtility.SaveFilePanel(
                    "Save your convert graph",
                    "=",
                    "NewConvertGraph.asset",
                    "asset"
                );

                if (string.IsNullOrEmpty(path)) return;
                string origianlPath = AssetDatabase.GUIDToAssetPath(convertGraph.guid);
                string destPath = $"Assets{path.Substring(Application.dataPath.Length)}";

                DoMove(origianlPath, destPath);
            }
        }

        private void DoMove(string origianlPath, string destPath)
        {
            var result = AssetDatabase.ValidateMoveAsset(origianlPath, destPath);
            if (string.IsNullOrEmpty(result))
            {
                AssetDatabase.MoveAsset(origianlPath, destPath);
            }
            else
            {
                EditorApplication.delayCall += () =>
                {
                    OnPostProcessMoveAssets(origianlPath, destPath);
                };
            }


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void OnPostProcessMoveAssets(string origianlPath, string destPath)
        {
            if (string.IsNullOrEmpty(origianlPath) || string.IsNullOrEmpty(destPath)) return;
            string res = AssetDatabase.ValidateMoveAsset(origianlPath, destPath);
            if (string.IsNullOrEmpty(res))
            {
                AssetDatabase.MoveAsset(origianlPath, destPath);
            }
            else
            {
                Debug.Log($"Unable to move file even in delay movement: {res}");
            }
        }

        public void SaveAsWithCopy() // Copy Asset first, but don't delete the old one 
        {
            // PLAN Add optional saving byhand
            if (graph is ConvertGraph convertGraph)
            {
                string path = EditorUtility.SaveFilePanel(
                    "Save convert graph",
                   "",
                    $"{convertGraph.name} copy.asset",
                    "asset"
                );

                if (string.IsNullOrEmpty(path)) return;
                string origianlPath = AssetDatabase.GUIDToAssetPath(convertGraph.guid);
                string destPath = $"Assets{path.Substring(Application.dataPath.Length)}";

                if (AssetDatabase.CopyAsset(origianlPath, destPath))
                {
                    EditorApplication.delayCall += () =>
                    {
                        var newGraph = AssetDatabase.LoadAssetAtPath<ConvertGraph>(destPath);
                        newGraph.guid = AssetDatabase.AssetPathToGUID(destPath);
                    };
                }
                else
                {
                    Debug.LogError($"Can't Move: dest path{destPath} from: {origianlPath}");
                }
            }
        }

        #endregion
    }
}
