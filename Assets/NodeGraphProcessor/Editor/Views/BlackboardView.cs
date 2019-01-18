using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using System.Linq;

namespace GraphProcessor
{
	public class BlackboardView : Blackboard
	{
		protected BaseGraphView	graphView;

		public BlackboardView(BaseGraphView baseGraphView)
		{
			this.graphView = baseGraphView;
			SetPosition(new Rect(0, 0, 100, 100));

            graphView.onExposedParameterListChanged += UpdateParameterList;
            graphView.initialized += UpdateParameterList;

            addItemRequested = (b) => OnAddClicked();
        }

        protected virtual void OnAddClicked()
        {
            graphView.graph.AddExposedParameter("New Param", 0.0f);
        }

        protected virtual void UpdateParameterList()
        {
            contentContainer.Clear();

            var row = new BlackboardRow(new BlackboardField{
                name = "TEST 1"
            }, new VisualElement{
                name = "PROP VIEW"
            });
            contentContainer.Add(row);

            foreach (var param in graphView.graph.exposedParameters)
            {
                row.Add(new BlackboardFieldView(param.name));
            }
        }
	}
}