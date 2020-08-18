using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using System.Text;
using GraphProcessor;
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
            var convertGraph = graph as ConvertGraph;
            int count = processList.Count;
            var inputs = processList[0].inputPorts;
            var outputs = processList[0].outputPorts;
            GraphNode startNode = null;
            int gNodesCount = 0;
            for (int i = 0; i < count; i++)
            {
                processList[i].OnProcess();
                if (processList[i] is ConvertNode is false)
                {
                    startNode = processList[i] as GraphNode;
                    gNodesCount++;
                }
            }

            if (startNode != null && gNodesCount == 1)
            {
                convertGraph.PostUpdateConvertGraph(startNode);
            }
            if (gNodesCount > 1)
            {
                Debug.LogWarning("Please delete extra primitive node since we only have one data node");
            }

            for (int i = 0; i < count; i++)
            {
                if (processList[i] is ConvertNode)
                {
                    var node = processList[i] as ConvertNode;
                    node.OnDelay();
                }

            }

            var sb = new StringBuilder();

            foreach (var cNode in convertGraph.CNodeList)
            {
                sb.Append(string.Format("{0}.Convert(", cNode.node.typeInfo.typeName));
                foreach (var cPortNode in cNode)
                {
                    if (cPortNode.portType != ConvertGraph.CPortType.ContainsValue)
                    {
                        if (cPortNode.portType == ConvertGraph.CPortType.outType)
                            sb.Append(string.Format(" out "));
                        if (cPortNode.portType != ConvertGraph.CPortType.dataType) sb.Append(string.Format("{0}  ", cPortNode.type.ToString().Split('.').LastOrDefault()));
                        sb.Append((char)('a' + cPortNode.sequence));
                    }

                    sb.Append(string.Format("{0}, ", cPortNode.treeNodeID));
                }
                sb.Append(");\n");
            }

            Debug.Log(sb.ToString());

            JobHandle.ScheduleBatchedJobs();
        }
    }
}
