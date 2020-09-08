using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;

namespace Cr7Sund.ConvertGraph
{
    [System.Serializable]
    public class GraphNode : BaseNode
    {

        #region Overwrite BaseNode

        [HideInInspector] public GraphNode parentNode;  // the nearest parnent with the highest graph level 
        [HideInInspector] public string treeID;
        public int graphLevel = 0;


        #endregion

        #region  Addition

        public IEnumerable<SerializableEdge> GetInputEdges()
        {
            foreach (var port in inputPorts)
                foreach (var edge in port.GetEdges())
                {
                    yield return edge;
                }
        }

        public IEnumerable<SerializableEdge> GetOutputEdges()
        {
            foreach (var port in outputPorts)
                foreach (var edge in port.GetEdges())
                {
                    yield return edge;
                }
        }

        #endregion 
    }


}