namespace Cr7Sund.ConvertGraph
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor.UIElements;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine.UIElements;
    using GraphProcessor;
    using System.Linq;

    [NodeCustomEditor(typeof(ConvertNode))]
    public class ConvertNodeView : BaseNodeView
    {
        protected override bool hasSettings => true;


        private ConvertNode dataNode;

        private string[] portFileKeys;
        public string[] PortFileKeys
        {
            get
            {
                if (portFileKeys == null)
                {
                    portFileKeys = portsPerFieldName.Keys.ToArray();
                }
                return portFileKeys;
            }
        }

        public override void Enable()
        {
            dataNode = nodeTarget as ConvertNode;

            var parameters = dataNode.ParameterInfos;

        }


    }
}