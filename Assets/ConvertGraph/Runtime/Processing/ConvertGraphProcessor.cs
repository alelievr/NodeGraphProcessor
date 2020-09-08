using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using System.Text;
using GraphProcessor;
using System;
// using Unity.Entities;

namespace Cr7Sund.ConvertGraph
{

    /// <summary>
    /// Graph processor
    /// </summary>
    public class ConvertGraphProcessor : BaseGraphProcessor
    {
        List<BaseNode> processList;

        /// <summary>
        /// Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public ConvertGraphProcessor(BaseGraph graph) : base(graph) { }

        public override void UpdateComputeOrder()
        {
            processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
        }

        /// <summary>
        /// Schedule the graph into the job system
        /// </summary>
        public override void Run()
        {
            int count = processList.Count;

            for (int i = 0; i < count; i++)
            {
                processList[i].OnProcess();
                if (processList[i] is ConvertNode node)
                {
                    node.OnDelay();
                }
            }
        }

        // avoid add extra node whose graph level is greater than the dest nodeF
        private static void RemoveNoConnectEndNdoe(SourceNode endNode, List<ConvertGraph.CNode> cNodeList)
        {
            int length = cNodeList.Count - 1;
            int removeIndex = length;
            int removeCount = 0;
            for (int i = length; i >= 0; i--)
            {
                if (cNodeList[i].node.graphLevel >= endNode.graphLevel)
                {
                    cNodeList[i].node.AddMessage("Please Delete this node since it's useless", NodeMessageType.Warning);
                    removeIndex = i;
                    removeCount++;
                }
                else
                {
                    break;
                }
            }

            if (removeIndex > 0)
            {
                cNodeList.RemoveRange(removeIndex, removeCount);
            }
        }

        private static StringBuilder PrintConvertInfo(List<ConvertGraph.CNode> cNodeList, out string lastInfo)
        {
            var sb = new StringBuilder();
            lastInfo = "a";

            for (int i = 0; i < cNodeList.Count; i++)
            {
                var cNode = cNodeList[i];

                sb.Append($"{cNode.node.classTypeInfo.fullName}.{cNode.node.convertFuncName}(");
                for (int j = 0; j < cNode.Count; j++)
                {
                    var cPortNode = cNode[j];

                    if (cPortNode.portType != ConvertGraph.CPortType.ContainsValue)
                    {
                        if (cPortNode.portType == ConvertGraph.CPortType.outType)
                            sb.Append(string.Format(" out "));
                        if (cPortNode.portType != ConvertGraph.CPortType.dataType) sb.Append(string.Format("{0}  ", cPortNode.type.ToString()));
                        sb.Append((char)('a' + cPortNode.sequence));
                    }

                    var postFix = j == cNode.Count - 1 ? "" : ",";
                    sb.Append($"{cPortNode.treeNodeID}{postFix}");

                    if (i == cNodeList.Count - 1) lastInfo = $"{(char)('a' + cPortNode.sequence)}{cPortNode.treeNodeID}{postFix}";
                }
                sb.Append(");\n");
            }
            return sb;
        }

        public string RunAndGenerateCode(out string lastInfo)
        {
            var convertGraph = graph as ConvertGraph;
            int count = processList.Count;
            SourceNode startNode = null;
            SourceNode endNode = null;

            for (int i = 0; i < count; i++)
            {
                processList[i].OnProcess();
                if (processList[i] is SourceNode sourceNode)
                {
                    if (sourceNode.graphNodeType == GraphNodeType.VNode)
                    {
                        startNode = sourceNode;
                    }
                    if (sourceNode.graphNodeType == GraphNodeType.MNode)
                    {
                        endNode = sourceNode;
                    }
                }
            }

            if (startNode != null)
            {
                convertGraph.PreUpdateConvertGraph(startNode);
                convertGraph.PostUpdateConvertGraph(startNode);
            }

            int sourceNodeCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (processList[i] is ConvertNode node)
                {
                    node.OnDelay();
                }
                else if (processList[i] is SourceNode)
                {
                    sourceNodeCount++;
                }
            }
            if (sourceNodeCount < 1)
            {
                throw new Exception("You mother fucker had delete the source node");
            }
            if (sourceNodeCount > 2)
            {
                throw new Exception("You have create three or more source node");
            }
            if (sourceNodeCount < 2)
            {
                Debug.LogWarning(" Ingorable Warning: It seems that you don't create a source node as ending one");
            }

            var cNodeList = convertGraph.CNodeList;
            RemoveNoConnectEndNdoe(endNode, cNodeList);
            var sb = PrintConvertInfo(cNodeList, out lastInfo);

            JobHandle.ScheduleBatchedJobs();

            return sb.ToString();
        }

    }
}
