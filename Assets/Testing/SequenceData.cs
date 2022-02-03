using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;


[Serializable]
public class SequenceName : List<SequenceName.ConditionalName>
{
    public string CurrentName => FindLast(x => x.IsAvailable()).Name;

    [Serializable]
    public class ConditionalName : BaseIsConditional
    {
        [SerializeField, Input("Name"), ShowAsDrawer] string name;
        public string Name => name;
    }
}

public static class ListHelpers
{
    public static string GetCurrentName(this List<SequenceName.ConditionalName> list)
    {
        return list.FindLast(x => x.IsAvailable()).Name;
    }

    public static IList<SerializableEdge> OrderByInputAttribute(this IList<SerializableEdge> edges, InputAttribute inputAttribute)
    {
        if (inputAttribute is MyInputAttribute)
        {
            switch ((inputAttribute as MyInputAttribute).SortType)
            {
                case InputSortType.POSITION_Y:
                    edges = edges.OrderBy(x => x.outputNode.position.y).ToList();
                    break;
            }
        }
        return edges;
    }

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