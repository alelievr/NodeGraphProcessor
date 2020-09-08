using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using UnityEditor;

namespace Cr7Sund.ConvertGraph
{
    [System.Serializable]
    public class SubGraphNode : GraphNode
    {

        [Input(name = "In Value")]
        public object inputValue;

        [Output(name = "Out Value")]
        public object outputValue;

        public override string name => SubConvertGraph.name;

        public string subGraphGUID;
        public SourceNodeInfo startNodeInfo, destNodeInfo;
        [SerializeField, HideInInspector] private List<SerialPortData> inputPortDatas = new List<SerialPortData>();
        [SerializeField, HideInInspector] private List<SerialPortData> outputPortDatas = new List<SerialPortData>();

        public SubGraph SubConvertGraph => AssetDatabase.LoadAssetAtPath<SubGraph>(AssetDatabase.GUIDToAssetPath(subGraphGUID));
        public SourceNode StartNode => (SourceNode)SubConvertGraph.nodesPerGUID[startNodeInfo.nodeGUID];
        public SourceNode DestNode => (SourceNode)SubConvertGraph.nodesPerGUID[destNodeInfo.nodeGUID];

        public List<SerialPortData> InputPortDatas { get => inputPortDatas; }
        public List<SerialPortData> OutputPortDatas { get => outputPortDatas; }



        protected override void Process()
        {
            if (string.IsNullOrEmpty(subGraphGUID)) return;
            // var graphProcessor = new ConvertGraphProcessor(SubConvertGraph);


            // StartNode.inputValue = inputValue;
            // // graphProcessor.Run();
            // outputValue = DestNode.outputValue;
        }

        [CustomPortBehavior(nameof(inputValue))]
        IEnumerable<PortData> GetPortsForInput(List<SerializableEdge> edges)
        {
            for (int i = 0; i < inputPortDatas.Count; i++)
            {
                var inputPort = inputPortDatas[i];
                yield return new PortData
                {
                    displayName = inputPort.displayName,
                    displayType = inputPort.displayType.DataType,
                    identifier = inputPort.identifier,
                    acceptMultipleEdges = false
                };
            }
        }

        [CustomPortBehavior(nameof(outputValue))]
        IEnumerable<PortData> GetPortsForOutput(List<SerializableEdge> edges)
        {
            for (int i = 0; i < outputPortDatas.Count; i++)
            {
                var outputPort = outputPortDatas[i];
                yield return new PortData
                {
                    displayName = outputPort.displayName,
                    displayType = outputPort.displayType.DataType,
                    identifier = outputPort.identifier,
                    acceptMultipleEdges = true
                };
            }
        }

        [CustomPortOutput(nameof(outputValue), typeof(object), allowCast = false)]
        void PopOutputs(List<SerializableEdge> outputEdges)
        {
            for (int i = 0; i < outputEdges.Count; i++)
            {
                outputEdges[i].passThroughBuffer = outputValue;
            }
        }

        public void AddInputPorData(SerialPortData portData) => inputPortDatas.Add(portData);
        public void AddOutputPorData(SerialPortData portData) => outputPortDatas.Add(portData);

    }

    [System.Serializable]
    public class SerialPortData
    {
        /// <summary>
        /// Display name on the node
        /// </summary>
        public string displayName;
        /// <summary>
        /// The type that will be used for coloring with the type stylesheet
        /// </summary>
        public TypeInfo displayType;
        /// <summary>
        /// port identifier
        /// </summary>
        public string identifier;

        /// <summary>
        /// The type that will be used for idenfified the other node guid
        public string guid;
        public Vector2 position;
    }
}