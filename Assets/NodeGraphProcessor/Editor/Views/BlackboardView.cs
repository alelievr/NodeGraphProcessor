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
	public class ExposedParameterView : Blackboard
	{
		protected BaseGraphView	graphView;

		new const string title = "Exposed Parameters";

		public ExposedParameterView(BaseGraphView baseGraphView)
		{
			this.graphView = baseGraphView;
			base.title = title;
			base.subTitle = "";
			SetPosition(new Rect(0, 0, 150, 300));
			scrollable = true;

            graphView.onExposedParameterListChanged += UpdateParameterList;
            graphView.initialized += UpdateParameterList;

            addItemRequested = (b) => OnAddClicked();
        }

        protected virtual void OnAddClicked()
        {
            graphView.graph.AddExposedParameter("New Param #" + graphView.graph.exposedParameters.Count, 0.0f);
        }

        protected virtual void UpdateParameterList()
        {
            contentContainer.Clear();

            foreach (var param in graphView.graph.exposedParameters)
            {
                contentContainer.Add(new ExposedParameterFieldView(param));
            }
        }
	}
}