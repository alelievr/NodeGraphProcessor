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
	public class ExposedParameterFieldView : BlackboardField
	{
		protected BaseGraphView	graphView;

		public ExposedParameter	parameter { get; private set; }

		public ExposedParameterFieldView(ExposedParameter param) : base(null, param.name, null)
		{
			parameter = param;
			this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
			// TODO: handle parameter renaming
        }

		void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenu.MenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Delete", (a) => shadow.parent.Remove(this), DropdownMenu.MenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
	}
}