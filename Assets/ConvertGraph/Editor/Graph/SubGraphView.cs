namespace Cr7Sund.ConvertGraph
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphProcessor;
    using UnityEditor;
    using UnityEngine;

    public class SubGraphView : BaseGraphView
    {
        public SubGraphView(EditorWindow window) : base(window) { }

        public void PasteGraph(string serializedData, List<string> oldGUIDs, SubGraphNode subGraphNode)
        {
            UnserializeAndPasteCallback(serializedData, oldGUIDs, subGraphNode);
        }

        void UnserializeAndPasteCallback(string serializedData, List<string> oldGUIDs, SubGraphNode subGraphNode)
        {
            List<SerialPortData> inputPortDatas = subGraphNode.InputPortDatas; List<SerialPortData> outputPortDatas = subGraphNode.OutputPortDatas;

            var data = JsonUtility.FromJson<CopyPasteHelper>(serializedData);
            Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();
            Dictionary<string, string> nodesMap = new Dictionary<string, string>();

            for (int i = 0; i < data.copiedNodes.Count; i++)
            {
                var serializedNode = data.copiedNodes[i];
                var node = JsonSerializer.DeserializeNode(serializedNode);

                if (node == null)
                    continue;

                //Call OnNodeCreated on the new fresh copied node
                node.OnNodeCreated();
                //And move a bit the new node
                node.position.position += new Vector2(20, 20);

                var newNodeView = AddNode(node);
                nodesMap.Add(oldGUIDs[i], newNodeView.nodeTarget.GUID);

                copiedNodesMap[node.GUID] = node;

                //Select the new node
                AddToSelection(nodeViewsPerNode[node]);
            }

            for (int i = 0; i < inputPortDatas.Count; i++)
            {
                var node = GraphNode.CreateFromType(typeof(SourceNode), inputPortDatas[i].position) as SourceNode;
                node.graphNodeType = GraphNodeType.VNode;
                node.nodeName = inputPortDatas[i].displayName;
                node.canChangeType = false;
                node.vTypeInfo = inputPortDatas[i].displayType;
                AddNode(node);

                nodesMap[inputPortDatas[i].guid] = node.GUID;
                copiedNodesMap[inputPortDatas[i].guid] = node;
            }

            for (int i = 0; i < outputPortDatas.Count; i++)
            {
                var node = GraphNode.CreateFromType(typeof(SourceNode), outputPortDatas[i].position) as SourceNode;
                node.graphNodeType = GraphNodeType.MNode;
                node.nodeName = outputPortDatas[i].displayName;
                node.canChangeType = false;
                node.mTypeInfo = outputPortDatas[i].displayType;

                AddNode(node);

                nodesMap[outputPortDatas[i].guid] = node.GUID;
                copiedNodesMap[outputPortDatas[i].guid] = node;
            }

            foreach (var serializedGroup in data.copiedGroups)
            {
                var group = JsonSerializer.Deserialize<Group>(serializedGroup);

                //Same than for node
                group.OnCreated();

                // try to centre the created node in the screen
                group.position.position += new Vector2(20, 20);

                var oldGUIDList = group.innerNodeGUIDs.ToList();
                group.innerNodeGUIDs.Clear();
                foreach (var guid in oldGUIDList)
                {
                    graph.nodesPerGUID.TryGetValue(guid, out var node);

                    // In case group was copied from another graph
                    if (node == null)
                    {
                        copiedNodesMap.TryGetValue(guid, out node);
                        group.innerNodeGUIDs.Add(node.GUID);
                    }
                    else
                    {
                        group.innerNodeGUIDs.Add(copiedNodesMap[guid].GUID);
                    }
                }

                AddGroup(group);
            }

            foreach (var oldEdge in data.copiedEdges)
            {
                var edge = JsonSerializer.Deserialize<SerializableEdge>(oldEdge);
                string outputDataNodeGUID = $"{edge.outputNodeGUID}-{edge.outputPortIdentifier}";
                string inputDataNodeGUID = $"{edge.inputNodeGUID}-{edge.inputPortIdentifier}";

                BaseNode oldOutputNode = null;
                BaseNode oldInputNode = null;
                string outputFieldName = edge.outputFieldName;
                string inputFieldName = edge.inputFieldName;


                // Find port of new nodes:
                if (!nodesMap.TryGetValue(edge.outputNodeGUID, out string outputNodeGUID))
                {
                    if (nodesMap.TryGetValue(outputDataNodeGUID, out outputNodeGUID))
                    {
                        if (!graph.nodesPerGUID.ContainsKey(outputNodeGUID)) continue;
                        edge.outputNode = graph.nodesPerGUID[outputNodeGUID];
                        if (edge.outputNode is SourceNode sNode) { outputFieldName = nameof(sNode.outputValue); }
                        edge.outputPort = edge.outputNode.GetPort(outputFieldName, "0");
                        copiedNodesMap.TryGetValue(edge.outputNode.GUID, out oldOutputNode);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (!graph.nodesPerGUID.ContainsKey(outputNodeGUID)) continue;
                    edge.outputNode = graph.nodesPerGUID[outputNodeGUID];
                    edge.outputPort = edge.outputNode.GetPort(outputFieldName, edge.outputPortIdentifier);
                    copiedNodesMap.TryGetValue(edge.outputNode.GUID, out oldOutputNode);
                }

                if (!nodesMap.TryGetValue(edge.inputNodeGUID, out string inputNodeGUID))
                {
                    if (nodesMap.TryGetValue(inputDataNodeGUID, out inputNodeGUID))
                    {
                        if (!graph.nodesPerGUID.ContainsKey(inputNodeGUID)) continue;
                        edge.inputNode = graph.nodesPerGUID[inputNodeGUID];
                        if (edge.inputNode is SourceNode sNode)
                        {
                            inputFieldName = nameof(sNode.inputValue);
                        }
                        edge.inputPort = edge.inputNode.GetPort(inputFieldName, "0");
                        copiedNodesMap.TryGetValue(edge.inputNode.GUID, out oldInputNode);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (!graph.nodesPerGUID.ContainsKey(inputNodeGUID)) continue;
                    edge.inputNode = graph.nodesPerGUID[inputNodeGUID];
                    edge.inputPort = edge.inputNode.GetPort(inputFieldName, edge.inputPortIdentifier);
                    copiedNodesMap.TryGetValue(edge.inputNode.GUID, out oldInputNode);
                }


                oldInputNode = oldInputNode ?? edge.inputNode; // Don't fucking delete that
                oldOutputNode = oldOutputNode ?? edge.outputNode;

                var inputPort = edge.inputPort;
                var outputPort = edge.outputPort;
                var newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);

                if (nodeViewsPerNode.ContainsKey(oldInputNode) && nodeViewsPerNode.ContainsKey(oldOutputNode))
                {
                    var edgeView = new EdgeView()
                    {
                        userData = newEdge,
                        input = nodeViewsPerNode[oldInputNode].GetPortViewFromFieldName(newEdge.inputFieldName, newEdge.inputPortIdentifier),
                        output = nodeViewsPerNode[oldOutputNode].GetPortViewFromFieldName(newEdge.outputFieldName, newEdge.outputPortIdentifier)
                    };

                    Connect(edgeView);
                }
            }
        }

        public void DeserializeEdge(SerializableEdge edge, string outputNodeGUID, string inputNodeGUID)
        {
            if (!graph.nodesPerGUID.ContainsKey(outputNodeGUID) || !graph.nodesPerGUID.ContainsKey(inputNodeGUID))
            {
                return;
            }

            edge.outputNode = graph.nodesPerGUID[outputNodeGUID];
            edge.inputNode = graph.nodesPerGUID[inputNodeGUID];
            edge.inputPort = edge.inputNode.GetPort(edge.inputFieldName, edge.inputPortIdentifier);
            edge.outputPort = edge.outputNode.GetPort(edge.outputFieldName, edge.outputPortIdentifier);
        }
    }
}