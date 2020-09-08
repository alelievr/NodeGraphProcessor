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
    public class ConvertNodeView : GraphNodeView
    {
        private ConvertNode dataNode;
        protected override bool hasSettings => true;

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

        private List<VisualElement> previews = new List<VisualElement>();
        public override void Enable()
        {
            dataNode = nodeTarget as ConvertNode;

            if (dataNode.classTypeInfo.isNull) throw new System.Exception("Assign the data first");
            // dataNode.onAfterEdgeConnected += ShowPreview;
            // dataNode.onAfterEdgeDisconnected += ShowPreview;
            dataNode.onProcessed += RunPreview;
        }

        public void ShowPreview(SerializableEdge edge)
        {
            if (owner.graph is ConvertGraph convertGraph)
            {
                var processList = owner.graph.nodes.OrderBy(n => n.computeOrder).ToList(); ;
                for (int i = 0; i < processList.Count; i++)
                {
                    processList[i].OnProcess();
                    if (dataNode == processList[i])
                    {
                        System.Reflection.ParameterInfo[] parameterInfos = dataNode.ParameterInfos;
                        int j = 0;
                        controlsContainer.Clear();
                        foreach (var item in parameterInfos)
                        {
                            if (item.IsOut)
                            {
                                var element = FieldFactory.CreateField(item.ParameterType, dataNode.outputs[j], null, item.Name);
                                j++;
                                controlsContainer.Add(element);
                                previews.Add(element);
                            }
                        }
                        break;
                    }
                }

                for (int i = 0; i < processList.Count; i++)
                {
                    if (processList[i] is ConvertNode node)
                    {
                        node.OnDelay();
                        if (dataNode == processList[i])
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void RunPreview()
        {
            System.Reflection.ParameterInfo[] parameterInfos = dataNode.ParameterInfos;
            int i = 0;
            controlsContainer.Clear();
            foreach (var item in parameterInfos)
            {
                if (item.IsOut)
                {
                    var element = FieldFactory.CreateField(item.ParameterType, dataNode.outputs[i], null, item.Name);
                    i++;
                    controlsContainer.Add(element);
                    previews.Add(element);
                }
            }
        }

        protected override VisualElement CreateSettingsView()
        {
            var settings = new VisualElement();
            var infoElement = new Label() { text = ($"Description: {dataNode.description ?? "No description"}") };
            settings.Add(infoElement);
            return settings;
        }
    }
}