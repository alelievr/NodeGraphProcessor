namespace Cr7Sund.ConvertGraph
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using GraphProcessor;
    using System;
    using System.Linq;

    [NodeCustomEditor(typeof(SourceNode))]
    public class SourceNodeView : GraphNodeView
    {
        protected override bool hasSettings => true;
        private SourceNode dataNode;
        private Action valueChangeCallback;

        public override void Enable()
        {
            dataNode = nodeTarget as SourceNode;

            CreateController();
        }

        private void CreateController()
        {
            switch (dataNode.graphNodeType)
            {
                case GraphNodeType.MNode:
                    CreateInputPort(dataNode.mTypeInfo);
                    break;
                case GraphNodeType.VNode:
                    CreateOutputPort(dataNode.vTypeInfo);
                    break;
                default:
                    CreateOutputPort(dataNode.vTypeInfo);
                    break;
            }
        }

        private void CreateOutputPort(TypeInfo typeInfo)
        {
            var element = FieldFactory.CreateField(typeInfo.DataType, dataNode.outputValue, (newValue) =>
            {
                owner.RegisterCompleteObjectUndo("Create TestNodeView " + typeInfo.fullName);
                dataNode.outputValue = newValue;
                NotifyNodeChanged();
                valueChangeCallback?.Invoke();
            }, $"{typeInfo.fullName.Split('.').LastOrDefault()}Value");
            controlsContainer.Add(element);
            style.width = 200;
        }

        private void CreateInputPort(TypeInfo typeInfo)
        {
            var element = FieldFactory.CreateField(typeInfo.DataType, dataNode.inputValue, (newValue) =>
            {
                owner.RegisterCompleteObjectUndo("Create TestNodeView " + typeInfo.fullName);
                dataNode.inputValue = newValue;
                NotifyNodeChanged();
                valueChangeCallback?.Invoke();
            }, $"{typeInfo.fullName.Split('.').LastOrDefault()}Value");
            controlsContainer.Add(element);

            style.width = 200;
        }

        static Dictionary<TypeEnum, Type> typeMap = new Dictionary<TypeEnum, Type>()
        {
            {TypeEnum.INT, typeof(int)},
            {TypeEnum.BOOL, typeof(bool)},
            {TypeEnum.LONG, typeof(long)},
            {TypeEnum.FLOAT, typeof(float)},
            {TypeEnum.DOUBLE,       typeof(double)},
            {TypeEnum.STRING, typeof(string)},
            {TypeEnum.BOUNDS, typeof(Bounds)},
            {TypeEnum.COLOR,        typeof(Color)},
            {TypeEnum.VECTOR2,   typeof(Vector2)},
            {TypeEnum.VECTOR2INT, typeof(Vector2Int)},
            {TypeEnum.VECTOR3,          typeof(Vector3) },
            {TypeEnum.VECTOR3INT, typeof(Vector3Int) },
            {TypeEnum.VECTOR4,          typeof(Vector4) },
            {TypeEnum.ANIMATIONCURVE,       typeof(AnimationCurve)},
            {TypeEnum.ENUM,         typeof(Enum)},
            {TypeEnum.GRADIENT, typeof(Gradient)},
            {TypeEnum.OBJECT,           typeof(UnityEngine.Object)},
            {TypeEnum.RECT, typeof(Rect)}
        };

        protected override VisualElement CreateSettingsView()
        {
            if (!dataNode.canChangeType) return new Button() { text = "Happy" };

            var settings = new VisualElement();
            bool isInput = dataNode.graphNodeType == GraphNodeType.MNode;
            bool isOut = dataNode.graphNodeType == GraphNodeType.VNode;

            EnumField enumFiled = new EnumField("Choose Type", dataNode.typeEnum);
            settings.Add(enumFiled);

            enumFiled.RegisterValueChangedCallback((v) =>
               {
                   TypeEnum gType = (TypeEnum)v.newValue;
                   owner.RegisterCompleteObjectUndo($"Chagne node Type{nameof(gType)}");
                   controlsContainer.Clear();
                   VisualElement element;
                   Type type = typeMap[gType];

                   if (isInput)
                   {
                       dataNode.mTypeInfo = new TypeInfo(type);
                       if (owner.graph is ConvertGraph cGraph) cGraph.destNodeTypeInfo = dataNode.mTypeInfo;

                       element = FieldFactory.CreateField(type, dataNode.inputValue, (newValue) =>
                       {
                           dataNode.inputValue = newValue;
                           NotifyNodeChanged();
                           valueChangeCallback?.Invoke();
                       }, null);
                   }
                   else if (isOut)
                   {
                       dataNode.vTypeInfo = new TypeInfo(type);
                       if (owner.graph is ConvertGraph cGraph) cGraph.startNodeTypeInfo = dataNode.vTypeInfo;

                       element = FieldFactory.CreateField(type, dataNode.inputValue, (newValue) =>
                       {
                           dataNode.inputValue = newValue;
                           NotifyNodeChanged();
                           valueChangeCallback?.Invoke();
                       }, null);
                   }
                   else
                   {
                       element = new VisualElement();
                       throw new Exception("Currently don't support that");
                   }

                   dataNode.typeEnum = gType;
                   dataNode.UpdatePortsForFieldLocal(isInput ? nameof(dataNode.inputValue) : nameof(dataNode.outputValue));
                   controlsContainer.Add(element);
               }
            );

            return settings;
        }
    }
}