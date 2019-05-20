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

        protected virtual void OnAddClicked()
        {
            var parameterType = new GenericMenu();

            foreach (var paramType in GetExposedParameterTypes())
            parameterType.AddItem(new GUIContent(paramType.Name), false, () => {
                string uniqueName = "New " + paramType.Name + "Param";
                object value = null;

                uniqueName = GetUniqueExposedPropertyName(uniqueName);

                if (paramType.IsValueType)
                {
                    value = Activator.CreateInstance(paramType);
                }
                graphView.graph.AddExposedParameter(uniqueName, paramType, value);
            });

            parameterType.ShowAsContext();
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
            // filter the slot types because we don't want generic types (i.e lists)
            foreach (var type in NodeProvider.GetSlotTypes())
            {
                if (type.IsGenericType)
                    continue ;

                yield return type;
            }
        }

        protected virtual void UpdateParameterList()
        {
            content.Clear();

            foreach (var param in graphView.graph.exposedParameters)
            {
                content.Add(new ExposedParameterFieldView(graphView, param));
            }
        }

        protected override void Initialize(BaseGraphView graphView)
        {
			this.graphView = graphView;
			base.title = title;
			scrollable = true;

            graphView.onExposedParameterListChanged += UpdateParameterList;
            graphView.initialized += UpdateParameterList;

            // Add exposed parameter button
            header.Add(new Button(OnAddClicked){
                text = "+"
            });
        }
    }
}