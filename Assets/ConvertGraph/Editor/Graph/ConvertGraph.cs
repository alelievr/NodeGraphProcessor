namespace Cr7Sund.ConvertGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphProcessor;
    using UnityEngine;

    public class ConvertGraph : BaseGraph
    {
        public void AddSourceDataNode(Type dataType)
        {
            //var pos = Vector2.zero;
            //var node = GraphNode.CreateFromType(typeof(TestNode), pos) as TestNode;
            //node.inputTypeInfo.typeName = dataType.ToString();
            //node.assemblyName = dataType.Assembly.FullName;
            //AddNode(node);
        }

        public void AddTestNode(Type dataType, bool isM2V)
        {
            var pos = Vector2.zero;
            var node = GraphNode.CreateFromType(typeof(TestNode), pos) as TestNode;
            node.graphNodeType = isM2V ? GraphNodeType.InputNode : GraphNodeType.OutputNode;
            node.nodeName = isM2V ? "InputNode" : "OutputNode";

            if (isM2V)
            {
                node.inputTypeInfo.typeName = dataType.ToString();
                node.inputTypeInfo.assemblyName = dataType.Assembly.FullName;
            }
            else
            {
                node.outputTypeInfo.typeName = dataType.ToString();
                node.outputTypeInfo.assemblyName = dataType.Assembly.FullName;
            }
            AddNode(node);
        }

        public void PreHandleGraphForGenerate()
        {
            var processList = nodes.OrderBy(n => n.computeOrder).ToList();
            int count = processList.Count;

            GraphNode startNode = null;
            int gNodesCount = 0;
            for (int i = 0; i < count; i++)
            {
                if ((processList[i] is ConvertNode is false))
                {
                    continue;
                }
                startNode = processList[i] as GraphNode;
                if (startNode != null)
                {
                    PostUpdateConvertGraph(startNode);
                    gNodesCount++;
                }
                if (gNodesCount > 1)
                {
                    throw new System.Exception("Please delete extra primitive node since we only have one data node");
                }
            }
        }

        #region  overwrite

        public override void UpdateComputeOrder(ComputeOrderType type = ComputeOrderType.BreadthFirst)
        {
            base.UpdateComputeOrder();

            int i = 0;
            GraphNode dataNode = null;
            foreach (var node in nodes)
            {
                if (node is ConvertNode is false)
                {
                    if (node is GraphNode)
                    {
                        dataNode = node as GraphNode;
                        i++;
                    }
                    if (i > 1) break;
                }
            }

            if (i == 1)
            {
                PreUpdateConvertGraph(dataNode);
            }

        }

        #endregion

        #region graphInfo
        [System.Serializable]
        public struct CPortNode
        {
            public string treeNodeID; // base Graph Level, e.g 1.2.1, but generate 121
            public int sequence; // the sequence of portlist, e.g. x, y, z
            public Type type; // The parameter type, e.g. int
            public CPortType portType;
        }

        public enum CPortType
        {
            inType,
            outType,
            ContainsValue,
            dataType,
        }

        [System.Serializable]
        public class CNode : List<CPortNode>
        {
            public ConvertNode node; // actually it's convertnode

            public CNode(ConvertNode node)
            {
                this.node = node;
            }
        }


        Dictionary<GraphNode, CNode> cNodeMap = new Dictionary<GraphNode, CNode>(); // Key: GUID
        HashSet<string> graphIDSet = new HashSet<string>();

        [SerializeField] private List<CNode> cNodeList;
        public List<CNode> CNodeList
        {
            get
            {
                cNodeList = cNodeMap.OrderBy(n => n.Key.graphLevel).Select(kvp => kvp.Value).ToList();
                return cNodeList;
            }
        }

        public void PostUpdateConvertGraph(GraphNode startNode)
        {
            // Don't care about loop circle since it has been checked in advanced 

            cNodeMap.Clear();

            TraversetGraphBFS(startNode, (node) =>
            {
                if (!cNodeMap.TryGetValue(node, out CNode cNode))
                {
                    cNode = new CNode(node);
                    cNodeMap.Add(node, cNode);
                }

                var inputPorts = node.inputPorts;
                for (int i = 0; i < inputPorts.Count; i++)
                {
                    var edges = inputPorts[i].GetEdges();
                    var cPNode = new CPortNode();

                    if (edges.Count < 1)
                    {
                        var fieldType = inputPorts[i].portData.displayType;
                        cPNode.type = default(Type); //excellent
                        cPNode.sequence = i;
                        cPNode.portType = CPortType.ContainsValue;

                        var fileListValue = inputPorts[i].fieldInfo.GetValue(inputPorts[i].fieldOwner);
                        if (inputPorts[i].fieldInfo.FieldType.IsGenericType)
                        {
                            var subInfos = fileListValue as List<object>;

                            if (subInfos[i] == null)
                            {
                                // if default(int) not int a = 2;
                                cPNode.treeNodeID = string.Format("default({0})", fieldType);
                            }
                            else
                            {
                                // if int a = 3
                                cPNode.treeNodeID = subInfos[i].ToString();
                            }
                        }
                    }
                    else
                    {
                        var inputNode = edges[0].outputNode as GraphNode;

                        cPNode.treeNodeID = inputNode.treeID;
                        cPNode.portType = CPortType.dataType;

                        cPNode.type = edges[0].outputPort.portData.displayType;
                        cPNode.sequence = i;

                    }

                    cNode.Add(cPNode);

                }

                var outputPorts = node.outputPorts;
                for (int i = 0; i < outputPorts.Count; i++)
                {
                    cNode.Add(new CPortNode()
                    {
                        treeNodeID = node.treeID,
                        type = outputPorts[i].portData.displayType,
                        sequence = i,
                        portType = CPortType.outType
                    });
                }

            });
        }

        public void PreUpdateConvertGraph(GraphNode startNode)
        {
            graphIDSet.Clear();

            TraversetGraphBFS(startNode, null, (curNode, sourceNode) =>
             {
                 int sourceLevel = sourceNode.graphLevel;
                 int parentLevel = 0;
                 if (curNode.parentNode != null) parentLevel = curNode.parentNode.graphLevel;
                 if (curNode.parentNode == null || parentLevel < sourceLevel)
                 {
                     curNode.parentNode = sourceNode;
                 }
                 curNode.graphLevel = Mathf.Max(sourceLevel + 1, curNode.graphLevel);

             });

            TraversetGraphBFS(startNode, (node) =>    // We also need to update node tree id. it consit of its last parent(which means the highest graph level) with its currently not duplicated level
            {
                var parent = node.parentNode;
                var sb = new System.Text.StringBuilder();

                // // It's 21, not 22; (the begin index always start from 1)
                int j = 0;
                do
                {
                    sb.Clear();
                    string treeID = parent.treeID;
                    sb.Append(treeID);
                    sb.Append(++j);
                } while (graphIDSet.Contains(sb.ToString()));
                node.treeID = sb.ToString();
                graphIDSet.Add(node.treeID);
            });
        }


        private void TraversetGraphBFS(GraphNode startNode, Action<ConvertNode> visitedOnceFunc = null, Action<ConvertNode, GraphNode> iterateEachtimeFunc = null)
        {
            var queue = new Queue<GraphNode>(nodes.Count);
            var set = new HashSet<string>();  // Shit .NetFramWork4.71.

            queue.Enqueue(startNode);
            startNode.treeID = "";
            startNode.graphLevel = 0;
            set.Add(startNode.GUID);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                IEnumerator<BaseNode> cur = node.GetOutputNodes().GetEnumerator();
                while (cur.MoveNext())
                {
                    if (cur.Current is ConvertNode)
                    {
                        var cNode = cur.Current as ConvertNode;

                        if (!set.Contains(cur.Current.GUID))
                        {
                            queue.Enqueue(cNode);
                            set.Add(cur.Current.GUID);
                            if (visitedOnceFunc != null)
                                visitedOnceFunc(cNode);
                        }
                        if (iterateEachtimeFunc != null)
                            iterateEachtimeFunc(cNode, node);

                    }
                }
            }
        }


        #endregion
    }
}