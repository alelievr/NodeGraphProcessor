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
        MNode,
        VNode,
        ValueNode
    }

    public enum TypeEnum
    {
        INT,
        BOOL,
        LONG,
        FLOAT,
        DOUBLE,
        STRING,
        BOUNDS,
        COLOR,
        VECTOR2INT,
        VECTOR2,
        VECTOR3INT,
        VECTOR3,
        VECTOR4,
        ANIMATIONCURVE,
        ENUM,
        GRADIENT,
        OBJECT,
        RECT
    }

    [System.Serializable]
    public struct TypeInfo
    {
        public string fullName;
        public string assemblyName;
        private Type dataType;


        public TypeInfo(Type type)
        {
            if (type == null)
            {
                this.fullName = string.Empty;
                this.assemblyName = string.Empty;
            }
            else
            {
                this.fullName = type.FullName;
                this.assemblyName = type.Assembly.FullName;
            }
            this.dataType = type;
        }

        public Type DataType
        {
            get
            {
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(assemblyName)) return null;
                if (dataType == null) dataType = Assembly.Load(assemblyName).GetType(fullName);
                return dataType;
            }
        }

        public bool isNull => string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(assemblyName) || DataType == null;

        public bool isNotNull => !isNull;
        public FieldInfo[] GetFieldInfos() => DataType.GetFields(BindingFlags.Public | BindingFlags.Instance);

    }

    [System.Serializable]
    public struct SourceNodeInfo
    {
        public TypeInfo vTypeInfo;
        public TypeInfo mTypeInfo;
        public string nodeGUID;

        public SourceNodeInfo(string guid, Type mType = null, Type vType = null)
        {
            nodeGUID = guid;
            mTypeInfo = new TypeInfo(mType);
            vTypeInfo = new TypeInfo(vType);
        }

        public bool IsNull => string.IsNullOrEmpty(nodeGUID) || (mTypeInfo.isNull && vTypeInfo.isNull);
        public bool IsNotNull => !IsNull;
    }

    [System.Serializable]
    public class SourceNode : GraphNode
    {
        [Output(name = "Output")]
        public object outputValue;
        [Input(name = "Input")]
        public object inputValue;
        public override string name => nodeName;
        public override string layoutStyle => graphNodeType == GraphNodeType.MNode ? "Styles/OutputData" : "Styles/InputData";

        public TypeInfo mTypeInfo;
        public TypeInfo vTypeInfo;

        public string nodeName;
        public GraphNodeType graphNodeType;
        [HideInInspector] public TypeEnum typeEnum;
        public bool canChangeType = true;


        [CustomPortBehavior(nameof(outputValue))]
        IEnumerable<PortData> GetPortsForOutput(List<SerializableEdge> edges)
        {
            if (graphNodeType == GraphNodeType.MNode) yield break;

            yield return new PortData
            {
                displayName = "InputData",
                displayType = vTypeInfo.DataType,
                identifier = "0",
                acceptMultipleEdges = true
            };

        }

        [CustomPortOutput(nameof(outputValue), typeof(object), allowCast = true)]
        void PopOutputs(List<SerializableEdge> outputEdges)
        {
            if (graphNodeType == GraphNodeType.MNode) return;

            for (int i = 0; i < outputEdges.Count; i++)
            {
                outputEdges[i].passThroughBuffer = outputValue;
            }
        }

        [CustomPortBehavior(nameof(inputValue))]
        IEnumerable<PortData> GetPortsForInput(List<SerializableEdge> edges)
        {
            if (graphNodeType != GraphNodeType.MNode) yield break;

            yield return new PortData
            {
                displayName = "OutputData",
                displayType = mTypeInfo.DataType,
                identifier = "0",
                acceptMultipleEdges = false
            };

        }

        [CustomPortInput(nameof(inputValue), typeof(object), allowCast = true)]
        void PutInputs(List<SerializableEdge> inputEdges)
        {
            if (graphNodeType != GraphNodeType.MNode) return;

            for (int i = 0; i < inputEdges.Count; i++)
            {
                inputValue = inputEdges[i].passThroughBuffer;
            }
        }
    }
}