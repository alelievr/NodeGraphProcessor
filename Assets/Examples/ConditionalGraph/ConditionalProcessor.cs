using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using GraphProcessor;

public class ConditionalProcessor : BaseGraphProcessor
{
    List< BaseNode >		processList;
    
    /// <summary>
    /// Manage graph scheduling and processing
    /// </summary>
    /// <param name="graph">Graph to be processed</param>
    public ConditionalProcessor(BaseGraph graph) : base(graph) {}

    public override void UpdateComputeOrder() => processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();

    /// <summary>
    /// Schedule the graph into the job system
    /// </summary>
    public override void Run()
    {
        int count = processList.Count;

        Debug.Log("TODO");
        for (int i = 0; i < count; i++)
        {
            // processList[i].OnProcess();
        }
    }
}
