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

		public BlackboardView(BaseGraphView baseGraphView, string title = "Blackboard")
		{
			this.graphView = baseGraphView;
			this.title = title;
			SetPosition(new Rect(0, 0, 100, 300));
			scrollable = true;

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

            foreach (var param in graphView.graph.exposedParameters)
            {
				Debug.Log("Exposed param: " + param.name);
                contentContainer.Add(new BlackboardFieldView(param.name));
            }
        }
	}
}