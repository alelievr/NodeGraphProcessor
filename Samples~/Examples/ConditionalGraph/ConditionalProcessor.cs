using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Debug = UnityEngine.Debug;

namespace NodeGraphProcessor.Examples
{
    public class ConditionalProcessor : BaseGraphProcessor
    {
        List< BaseNode >		processList;
        List< StartNode >		startNodeList;

        Dictionary<BaseNode, List<BaseNode>>    nonConditionalDependenciesCache = new Dictionary<BaseNode, List<BaseNode>>();

        public bool             pause;

        public IEnumerator<BaseNode> currentGraphExecution { get; private set; } = null;

        // static readonly float   maxExecutionTimeMS = 100; // 100 ms max execution time to avoid infinite loops

        /// <summary>
        /// Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public ConditionalProcessor(BaseGraph graph) : base(graph) {}

        public override void UpdateComputeOrder()
        {
            // Gather start nodes:
            startNodeList = graph.nodes.Where(n => n is StartNode).Select(n => n as StartNode).ToList();

            // In case there is no start node, we process the graph like usual
            if (startNodeList.Count == 0)
            {
                processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
            }
            else
            {
                nonConditionalDependenciesCache.Clear();
                // Prepare the cache of non-conditional node execution
            }
        }

        public override void Run()
        {
            // Execute the whole graph:
            var enumerator = RunConditionalGraph();

            while (enumerator.MoveNext())
                ;
        }

        IEnumerable<BaseNode> GatherNonConditionalDependencies(BaseNode node)
        {
            Stack<BaseNode> dependencies = new Stack<BaseNode>();

            dependencies.Push(node);
        
            while (dependencies.Count > 0)
            {
                var dependency = dependencies.Pop();

                foreach (var d in dependency.GetInputNodes().Where(n => !(n is IConditionalNode)))
                    dependencies.Push(d);

                if (dependency != node)
                    yield return dependency;
            }
        }

        public IEnumerator<BaseNode> RunConditionalGraph()
        {
            if (startNodeList.Count == 0)
            {
                int count = processList.Count;

                for (int i = 0; i < count; i++)
                {
                    processList[i].OnProcess();
                    yield return processList[i];
                }
            }
            else // Conditional graph execution:
            {
                Stack<BaseNode>     nodeToExecute = new  Stack<BaseNode>();
                HashSet<BaseNode>   nodeDependenciesGathered = new HashSet<BaseNode>();
                HashSet<BaseNode>   skipConditionalHandling = new HashSet<BaseNode>();

                // Add all the start nodes to the execution stack
                startNodeList.ForEach(s => nodeToExecute.Push(s));

                while (nodeToExecute.Count > 0)
                {
                    var node = nodeToExecute.Pop();
                    // TODO: maxExecutionTimeMS

                    // In case the node is conditional, then we need to execute it's non-conditional dependencies first
                    if (node is IConditionalNode && !skipConditionalHandling.Contains(node))
                    {
                        // Gather non-conditional deps: TODO, move to the cache:
                        if (nodeDependenciesGathered.Contains(node))
                        {
                            // Execute the conditional node:
                            node.OnProcess();
                            yield return node;

                            // And select the next nodes to execute:
                            switch (node)
                            {
                                // special code path for the loop node as it will execute multiple times the same nodes
                                case ForLoopNode forLoopNode:
                                    forLoopNode.index = forLoopNode.start - 1; // Initialize the start index
                                    foreach (var n in forLoopNode.GetExecutedNodesLoopCompleted())
                                        nodeToExecute.Push(n);
                                    for (int i = forLoopNode.start; i < forLoopNode.end; i++)
                                    {
                                        foreach (var n in forLoopNode.GetExecutedNodesLoopBody())
                                            nodeToExecute.Push(n);

                                        nodeToExecute.Push(node); // Increment the counter
                                    }
                                    skipConditionalHandling.Add(node);
                                    break;
                                case IConditionalNode cNode:
                                    foreach (var n in cNode.GetExecutedNodes())
                                        nodeToExecute.Push(n);
                                    break;
                                default:
                                    Debug.LogError($"Conditional node {node} not handled");
                                    break;
                            }
                            nodeDependenciesGathered.Remove(node);
                        }
                        else
                        {
                            nodeToExecute.Push(node);
                            nodeDependenciesGathered.Add(node);
                            foreach (var nonConditionalNode in GatherNonConditionalDependencies(node))
                            {
                                nodeToExecute.Push(nonConditionalNode);
                            }
                        }
                    }
                    else
                    {
                        node.OnProcess();
                        yield return node;
                    }
                }
            }
        }

        // Advance the execution of the graph of one node, mostly for debug
        public void Step()
        {
            if (currentGraphExecution == null)
            {
                currentGraphExecution = RunConditionalGraph();
                currentGraphExecution.MoveNext(); // Advance to the first node
            }
            else
            if (!currentGraphExecution.MoveNext())
                currentGraphExecution = null;
        }
    }
}
