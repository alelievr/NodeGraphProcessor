using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;

namespace GraphProcessor
{
	public class ExposedParameterFieldView : BlackboardField
	{
		protected BaseGraphView	graphView;

		public ExposedParameter	parameter { get; private set; }

		public ExposedParameterFieldView(BaseGraphView graphView, ExposedParameter param) : base(null, param.name, "")
		{
			this.graphView = graphView;
			parameter = param;
			this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

			(this.Q("textField") as TextField).RegisterValueChangedCallback((e) => {
				param.name = e.newValue;
				text = e.newValue;
				graphView.graph.UpdateExposedParameterName(param, e.newValue);
			});
        }

		void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Delete", (a) => parent.Remove(this), DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
	}
}