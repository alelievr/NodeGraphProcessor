using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;

namespace Cr7Sund.ConvertGraph
{
    [System.Serializable]
    public class ConvertNode : GraphNode
    {
        private Func<List<object>, object> converting;

        [HideInInspector] public string convertFuncName;
        [HideInInspector] public TypeInfo classTypeInfo;
        [HideInInspector] public string description;


        public Type convertClassType => Assembly.Load(classTypeInfo.assemblyName).GetType(classTypeInfo.fullName);
        protected MethodInfo MInfo => convertClassType.GetMethod(convertFuncName);
        public ParameterInfo[] ParameterInfos => MInfo.GetParameters();

        [Input(name = "In Values", allowMultiple = true)]
        public List<object> inputs = new List<object>();

        [Output(name = "Out Valus", allowMultiple = true)]
        public List<object> outputs = new List<object>();

        private int defaultIndex = -1;
        private int outputIndex = 0;

        public override string name => convertFuncName;

        protected override void Process()
        {
            //PLAN replace method invoke by delegate https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/
            // converting = (Func<List<object>, object>)
            //              Delegate.CreateDelegate(typeof(Func<List<object>, object>), null, method);
            // value = converting(inputs);


            var paramList = new List<object>();
            int i = 0;
            int outputLength = ParameterInfos.Length - inputs.Count;
            int inputIndex = defaultIndex == -1 ? inputs.Count : defaultIndex - outputLength;


            for (i = 0; i < inputIndex; i++)
            {
                paramList.Add(inputs[i]);
            }

            for (; i < inputIndex + outputLength; i++)
            {
                paramList.Add(null);
            }
            for (; i < ParameterInfos.Length; i++)
            {
                if (inputs[i - outputLength] == null)
                {
                    paramList.Add(ParameterInfos[i].DefaultValue);
                }
                else
                {
                    paramList.Add(inputs[i - outputLength]);
                }
            }

            object[] parameters = paramList.ToArray();
            try
            {
                MInfo.Invoke(null, parameters);
            }
            catch
            {
                // PLAN a message center to handle warning and error
                AddMessage("The input data is null or there is wrong with your converter logic", NodeMessageType.Warning);  // Maybe you are assuming your test value is been delete
            }

            for (i = inputIndex; i < inputIndex + outputLength; i++)
            {
                outputs.Add(parameters[i]);
            }

        }

        [CustomPortBehavior(nameof(inputs))]
        IEnumerable<PortData> GetPortsForInputs(List<SerializableEdge> edges)
        {
            defaultIndex = -1;

            int sequence = 0;
            for (int i = 0; i < ParameterInfos.Length; i++)
            {
                if (!ParameterInfos[i].IsOut)
                {
                    if (defaultIndex == -1 && ParameterInfos[i].HasDefaultValue)
                    {
                        defaultIndex = i;
                    }

                    yield return new PortData
                    {
                        displayName = ParameterInfos[i].Name,
                        displayType = ParameterInfos[i].ParameterType,
                        identifier = (sequence++).ToString(),
                    };
                }
            }

        }

        [CustomPortInput(nameof(inputs), typeof(object), allowCast = true)]
        void PullInputs(List<SerializableEdge> inputEdges)
        {
            if (inputEdges.Count < 1)
            {
                inputs.Add(null);
            }
            else
            {
                inputs.AddRange(inputEdges.Select(e => e.passThroughBuffer).ToList());
            }

        }

        [CustomPortBehavior(nameof(outputs))]
        IEnumerable<PortData> GetPortsForOutput(List<SerializableEdge> edges)
        {
            int sequence = 0;
            for (int i = 0; i < ParameterInfos.Length; i++)
            {
                if (ParameterInfos[i].IsOut)
                {
                    yield return new PortData
                    {
                        displayName = ParameterInfos[i].Name,
                        displayType = ParameterInfos[i].ParameterType.GetElementType(), // It's actually be a pointer for the type. ref: http://www.blackwasp.co.uk/ReflectOutRefParams_2.aspx
                        identifier = (sequence++).ToString(),
                        acceptMultipleEdges = true
                    };
                }
            }

        }

        [CustomPortOutput(nameof(outputs), typeof(object), allowCast = true)]
        void PopOutputs(List<SerializableEdge> outputEdges)
        {

            for (int i = 0; i < outputEdges.Count; i++)
            {
                try
                {
                    outputEdges[i].passThroughBuffer = outputs[outputIndex];
                }
                catch
                {
                    throw new Exception(classTypeInfo.fullName);
                }
            }

            if (++outputIndex == outputs.Count)
            {
                outputIndex = 0;
            }

        }

        public void OnDelay()
        {
            outputIndex = 0;
            inputs.Clear();
            outputs.Clear();
        }

    }
}