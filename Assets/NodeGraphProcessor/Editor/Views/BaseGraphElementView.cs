using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor;
using UnityEngine;

namespace GraphProcessor
{
	//Waiting for Unity's 2018.2 BlackBoard
	public abstract class BaseGraphElementView : Node
	{
		public void InitializeGraphView(BaseGraphView graphView)
		{
			SetSize(Vector2.one * 50);

			style.backgroundColor = Color.red;

			Initialize(graphView);

			RefreshPorts();
		}

		protected abstract void Initialize(BaseGraphView graphView);

		~BaseGraphElementView()
		{
			Destroy();
		}

		protected virtual void Destroy() {}
	}
}