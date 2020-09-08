namespace Cr7Sund.ConvertGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphProcessor;
    using UnityEngine;

    public class ConvertGraph : BaseGraph
    {
        /// <summary>
        /// Create Source Node, but you should add by yourself. On BaseNode or BaseNodeView
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="isM2V"></param>
        /// <param name="pos"></param>
        /// <param name="isDynamicType"></param>
        /// <returns></returns>



        #region  Addition

        public TypeInfo startNodeTypeInfo;
        public TypeInfo destNodeTypeInfo;
        public string helpUrl = "https://store.steampowered.com/";
        public string guid;
        // public bool isM2V;

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

            TraversetGraphBFS(startNode, (gNode) =>
            {
                if (gNode is ConvertNode node)
                {
                    var defaultNodes = new List<CPortNode>(); // Dealay add;
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
                        bool isDefault = false;

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

                                if (subInfos.Count < 1)
                                {
                                    throw new Exception("the input or output had been deleted ");
                                }
                                if (subInfos[i] == null)
                                {
                                    // if default(int) not int a = 2;
                                    cPNode.treeNodeID = string.Format("default", fieldType);
                                    isDefault = true;
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

                        if (isDefault)
                            defaultNodes.Add(cPNode);
                        else
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

                    for (int i = 0; i < defaultNodes.Count; i++)
                    {
                        cNode.Add(defaultNodes[i]);
                    }
                }

            });
        }

        public void PreUpdateConvertGraph(GraphNode startNode)
        {
            graphIDSet.Clear();

            UpdateGraphLevel(startNode);

            UpdateTreeID(startNode);
        }

        private void UpdateTreeID(GraphNode startNode)
        {
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

        private void UpdateGraphLevel(GraphNode startNode)
        {
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
        }

        private void TraversetGraphBFS(GraphNode startNode, Action<GraphNode> visitedOnceFunc = null, Action<GraphNode, GraphNode> iterateEachtimeFunc = null)
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
                    if (cur.Current is GraphNode gNode)
                    {

                        if (!set.Contains(cur.Current.GUID))
                        {
                            queue.Enqueue(gNode);
                            set.Add(cur.Current.GUID);
                            if (visitedOnceFunc != null)
                                visitedOnceFunc(gNode);
                        }
                        if (iterateEachtimeFunc != null)
                            iterateEachtimeFunc(gNode, node);

                    }
                }
            }
        }


        #endregion
    }
}