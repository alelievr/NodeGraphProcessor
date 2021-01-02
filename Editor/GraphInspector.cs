using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace GraphProcessor
{
    public class GraphInspector : Editor
    {
        protected VisualElement root;
        protected BaseGraph     graph;
        protected ExposedParameterFieldFactory exposedParameterFactory;

        VisualElement           parameterContainer;

        protected virtual void OnEnable()
        {
            graph = target as BaseGraph;
            graph.onExposedParameterListChanged += UpdateExposedParameters;
            graph.onExposedParameterModified += UpdateExposedParameters;
            exposedParameterFactory = new ExposedParameterFieldFactory(graph);
        }

        protected virtual void OnDisable()
        {
            graph.onExposedParameterListChanged -= UpdateExposedParameters;
            graph.onExposedParameterModified -= UpdateExposedParameters;
            exposedParameterFactory.Dispose();
        }

        public sealed override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            CreateInspector();
            return root;
        }

        protected virtual void CreateInspector()
        {
            parameterContainer = new VisualElement{
                name = "ExposedParameters"
            };
            FillExposedParameters(parameterContainer);

            root.Add(parameterContainer);
        }

        protected void FillExposedParameters(VisualElement parameterContainer)
        {
            if (graph.exposedParameters.Count != 0)
                parameterContainer.Add(new Label("Exposed Parameters:"));

            SerializedObject so = new SerializedObject(graph);
            var exposedParameters = so.FindProperty(nameof(graph.exposedParameters));
            
            foreach (var param in graph.exposedParameters)
            {
                var field = exposedParameterFactory.GetParameterValueField(param, (newValue) => {
                    Debug.Log("changed: " + param.value + " => " + newValue);
                    param.value = newValue;
                    serializedObject.ApplyModifiedProperties();
                    graph.NotifyExposedParameterValueChanged(param);
                });
                parameterContainer.Add(field);
            }
        }

        void UpdateExposedParameters(ExposedParameter param) => UpdateExposedParameters();

        void UpdateExposedParameters()
        {
            parameterContainer.Clear();
            FillExposedParameters(parameterContainer);
        }

        // Don't use ImGUI
        public sealed override void OnInspectorGUI() {}

    }
}