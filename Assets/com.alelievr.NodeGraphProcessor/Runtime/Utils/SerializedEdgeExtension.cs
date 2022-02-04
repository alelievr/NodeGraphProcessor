using System.Collections.Generic;

namespace GraphProcessor
{
    public static class SerializedEdgeExtension
    {
        public static IList<SerializableEdge> GetNonRelayEdges(this IList<SerializableEdge> edges)
        {
            List<SerializableEdge> nonrelayEdges = new List<SerializableEdge>();
            foreach (var edge in edges)
            {
                if (edge.outputNode is RelayNode)
                {
                    RelayNode relay = edge.outputNode as RelayNode;
                    foreach (var relayEdge in relay.GetNonRelayEdges())
                    {
                        nonrelayEdges.Add(relayEdge);
                    }
                }
                else
                {
                    nonrelayEdges.Add(edge);
                }
            }
            return nonrelayEdges;
        }
    }
}
