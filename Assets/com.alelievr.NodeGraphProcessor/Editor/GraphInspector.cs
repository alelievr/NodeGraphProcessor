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

        VisualElement   parameterContainer;

        protected virtual void OnEnable()
        {
            graph = target as BaseGraph;
            graph.onExposedParameterListChanged += UpdateExposedParameters;
            graph.onExposedParameterModified += UpdateExposedParameters;
        }

        protected virtual void OnDisable()
        {
            graph.onExposedParameterListChanged -= UpdateExposedParameters;
            graph.onExposedParameterModified -= UpdateExposedParameters;
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

            for (int i = 0; i < exposedParameters.arraySize; i++)
            {
                var param = exposedParameters.GetArrayElementAtIndex(i);
                var value = graph.exposedParameters[i];

                if (value.settings.isHidden)
                    continue;

                var p = new PropertyField(param);
                p.Bind(so);
                parameterContainer.Add(p);
            }
        }

        void UpdateExposedParameters(string guid) => UpdateExposedParameters();
		
        void UpdateExposedParameters()
        {
            parameterContainer.Clear();
            FillExposedParameters(parameterContainer);
        }

        // Don't use ImGUI
        public sealed override void OnInspectorGUI() {}

    }
}