using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor;
using UnityEngine;

namespace GraphProcessor
{
	public abstract class BaseGraphElementView : GraphElement
	{
		public void InitializeGraphView(BaseGraphView graphView)
		{
			SetSize(Vector2.one * 50);
			style.positionType = PositionType.Absolute;
			capabilities |= Capabilities.Resizable;

			AddStyleSheetPath("StyleSheets/GraphView/Node.uss");

			Initialize(graphView.graph);
		}

		public abstract void Initialize(BaseGraph graph);
	}
}