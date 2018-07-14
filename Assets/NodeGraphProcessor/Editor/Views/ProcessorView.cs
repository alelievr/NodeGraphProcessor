using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
	public class ProcessorView : PinnedElementView
	{
		BaseGraphProcessor	processor;

		public ProcessorView()
		{
			title = "Process panel";
		}

		protected override void Initialize(BaseGraphView graphView)
		{
			processor = new ProcessGraphProcessor(graphView.graph);

			graphView.computeOrderUpdated += processor.UpdateComputeOrder;

			Button	b = new Button(OnPlay) { name = "ActionButton", text = "Play !" };

			Add(b);
		}

		void OnPlay()
		{
			processor.Run();
		}
	}
}
