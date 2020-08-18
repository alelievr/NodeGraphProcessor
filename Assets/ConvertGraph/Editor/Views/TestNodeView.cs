namespace Cr7Sund.ConvertGraph
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using GraphProcessor;
    using System;

    [NodeCustomEditor(typeof(TestNode))]
    public class TestNodeView : BaseNodeView
    {
        protected override bool hasSettings => true;

        private const string Str = "nothing";
        private TestNode dataNode;
        private Action valueChangeCallback;
        public override void Enable()
        {
            dataNode = nodeTarget as TestNode;

            TypeInfo typeInfo;
            switch (dataNode.graphNodeType)
            {
                case GraphNodeType.InputNode:
                    CreateInputPort(dataNode.inputTypeInfo);
                    break;
                case GraphNodeType.OutputNode:
                    CreateOutputPort(dataNode.outputTypeInfo);
                    break;
                default:
                    CreateOutputPort(dataNode.outputTypeInfo);
                    break;
            }
            return;

            // Register valueChange to start process
        }

        private void CreateOutputPort(TypeInfo typeInfo)
        {
            var element = FieldFactory.CreateField(typeInfo.DataType, dataNode.outputValue, (newValue) =>
            {
                owner.RegisterCompleteObjectUndo("Create TestNodeView " + typeInfo.typeName);
                dataNode.inputValue = newValue;
                NotifyNodeChanged();
                valueChangeCallback?.Invoke();
            }, null);
            controlsContainer.Add(element);
            style.width = 200;
        }

        private void CreateInputPort(TypeInfo typeInfo)
        {
            var element = FieldFactory.CreateField(typeInfo.DataType, dataNode.inputValue, (newValue) =>
            {
                owner.RegisterCompleteObjectUndo("Create TestNodeView " + typeInfo.typeName);
                dataNode.inputValue = newValue;
                NotifyNodeChanged();
                valueChangeCallback?.Invoke();
            }, null);
            controlsContainer.Add(element);
            style.width = 200;
        }

        protected override VisualElement CreateSettingsView()
        {
            var settings = new VisualElement();

            EnumField enumFiled = new EnumField("Choose Type", dataNode.graphNodeType);
            settings.Add(enumFiled);

            return settings;
        }
    }
}