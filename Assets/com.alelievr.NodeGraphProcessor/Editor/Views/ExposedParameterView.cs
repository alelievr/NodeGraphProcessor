using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using System;

namespace GraphProcessor
{
	public class ExposedParameterView : PinnedElementView
	{
		protected BaseGraphView	graphView;

		new const string title = "Parameters";
        
        readonly string exposedParameterViewStyle = "GraphProcessorStyles/ExposedParameterView";

        public ExposedParameterView()
        {
            var style = Resources.Load<StyleSheet>(exposedParameterViewStyle);
            if (style != null)
                styleSheets.Add(style);
        }

        protected virtual void OnAddClicked()
        {
            var parameterType = new GenericMenu();

            foreach (var paramType in GetExposedParameterTypes())
                parameterType.AddItem(new GUIContent(GetNiceNameFromType(paramType)), false, () =>
                {
                    string uniqueName = "New " + GetNiceNameFromType(paramType);

                    uniqueName = GetUniqueExposedPropertyName(uniqueName);
                    graphView.graph.AddExposedParameter(uniqueName, paramType);
                });

            parameterType.ShowAsContext();
        }

        protected string GetNiceNameFromType(Type type)
        {
            string name = type.Name;

            // Remove parameter in the name of the type if it exists
            name = name.Replace("Parameter", "");

            return ObjectNames.NicifyVariableName(name);
        }

        protected string GetUniqueExposedPropertyName(string name)
        {
            // Generate unique name
            string uniqueName = name;
            int i = 0;
            while (graphView.graph.exposedParameters.Any(e => e.name == name))
                name = uniqueName + " " + i++;
            return name;
        }

        protected virtual IEnumerable< Type > GetExposedParameterTypes()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<ExposedParameter>())
            {
                if (type.IsGenericType)
                    continue ;

                yield return type;
            }
        }

        protected virtual void UpdateParameterList()
        {
            content.Clear();

            for (int i = 0; i < graphView.serializedParameters.arraySize; i++)
            {
                var param = graphView.graph.exposedParameters[i];
                var serializedParam = graphView.serializedParameters.GetArrayElementAtIndex(i);

                var row = new BlackboardRow(new ExposedParameterFieldView(graphView, param), new ExposedParameterPropertyView(graphView, param, serializedParam));
                row.expanded = param.settings.expanded;
                row.RegisterCallback<GeometryChangedEvent>(e => {
                    param.settings.expanded = row.expanded;
                });

                content.Add(row);
            }
        }

        protected override void Initialize(BaseGraphView graphView)
        {
			this.graphView = graphView;
			base.title = title;
			scrollable = true;

            graphView.onExposedParameterListChanged += UpdateParameterList;
            graphView.initialized += UpdateParameterList;

            UpdateParameterList();

            // Add exposed parameter button
            header.Add(new Button(OnAddClicked){
                text = "+"
            });
        }
    }
}