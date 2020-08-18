namespace Cr7Sund.ConvertGraph
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GraphProcessor;
    using System.Linq;
    using System;
    using System.Reflection;

    public enum GraphNodeType
    {
        InputNode,
        OutputNode,
        ValueNode
    }

    [System.Serializable]
    public struct TypeInfo
    {
        public string typeName;
        public string assemblyName;
        private Type dataType;
        public Type DataType
        {
            get
            {
                if (dataType == null) dataType = Assembly.Load(assemblyName).GetType(typeName);
                return dataType;
            }
        }

        public FieldInfo[] GetFieldInfos() => DataType.GetFields(BindingFlags.Public | BindingFlags.Instance);
    }

    [System.Serializable]
    public class TestNode : GraphNode
    {
        [Output(name = "Output")]
        public object outputValue;
        [Input(name = "Input")]
        public object inputValue;
        public override string name => nodeName;


        public TypeInfo inputTypeInfo;
        public TypeInfo outputTypeInfo;

        public string nodeName;
        public GraphNodeType graphNodeType;
        public bool canChangeType;


        [CustomPortBehavior(nameof(outputValue))]
        IEnumerable<PortData> GetPortsForOutput(List<SerializableEdge> edges)
        {
            if (graphNodeType == GraphNodeType.InputNode) yield break;
            var fields = outputTypeInfo.GetFieldInfos();

            for (int i = 0; i < fields.Length; i++)
            {
                yield return new PortData
                {
                    displayName = fields[i].Name,
                    displayType = fields[i].FieldType,
                    identifier = i.ToString(),
                    acceptMultipleEdges = true
                };
            }
            if (fields.Length == 0)
            {
                yield return new PortData
                {
                    displayName = "InputData",
                    displayType = outputTypeInfo.DataType,
                    identifier = "0",
                    acceptMultipleEdges = true
                };
            }
        }

        [CustomPortOutput(nameof(outputValue), typeof(object), allowCast = true)]
        void SeparateDataForOutputs(List<SerializableEdge> outputEdges)
        {
            if (graphNodeType == GraphNodeType.InputNode) return;

            // TODO: inject Real data or simulate data
            for (int i = 0; i < outputEdges.Count; i++)
            {
                outputEdges[i].passThroughBuffer = outputValue;
            }
        }

        [CustomPortBehavior(nameof(inputValue))]
        IEnumerable<PortData> GetPortsForInput(List<SerializableEdge> edges)
        {
            if (graphNodeType != GraphNodeType.InputNode) yield break;

            var fields = inputTypeInfo.GetFieldInfos();
            for (int i = 0; i < fields.Length; i++)
            {
                yield return new PortData
                {
                    displayName = fields[i].Name,
                    displayType = fields[i].FieldType,
                    identifier = i.ToString(),
                    acceptMultipleEdges = true
                };
            }
            if (fields.Length == 0)
            {
                yield return new PortData
                {
                    displayName = "OutputData",
                    displayType = inputTypeInfo.DataType,
                    identifier = "0",
                    acceptMultipleEdges = true
                };
            }
        }

        [CustomPortOutput(nameof(inputValue), typeof(object), allowCast = true)]
        void SeparateDataForInputs(List<SerializableEdge> inputEdges)
        {
            if (graphNodeType != GraphNodeType.InputNode) return;

            for (int i = 0; i < inputEdges.Count; i++)
            {
                inputEdges[i].passThroughBuffer = inputValue;
            }
        }
    }
}