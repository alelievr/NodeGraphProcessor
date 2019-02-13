using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;

namespace GraphProcessor
{
    public class GraphInspector : Editor
    {
        protected VisualElement root;
        protected BaseGraph     graph;

        void OnEnable()
        {
            graph = target as BaseGraph;
            graph.onExposedParameterListChanged += UpdateExposedParameters;
        }

        void OnDisable()
        {
            graph.onExposedParameterListChanged -= UpdateExposedParameters;
        }

        public sealed override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            CreateInspector();
            return root;
        }

        protected virtual void CreateInspector()
        {
            root.Add(CreateExposedParameters());
        }

        protected VisualElement CreateExposedParameters()
        {
            VisualElement parameters = new VisualElement{
                name = "ExposedParameters"
            };

            parameters.Add(new Label("Exposed Parameters"));
            foreach (var param in graph.exposedParameters)
            {
                Type paramType = Type.GetType(param.type);
                var field = FieldFactory.CreateField(paramType);
                // TODO: assign field text so we see the name of the parameter
                parameters.Add(field);
            }

            return parameters;
        }

        void UpdateExposedParameters()
        {

        }

        // Don't use ImGUI
        public sealed override void OnInspectorGUI() {}

    }
}