using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
	//TODO: change this to a blackboard in 2018.2
	public class ProcessorView : BaseGraphElementView
	{
		BaseGraphProcessor	processor;

		public ProcessorView()
		{
		}

		public override void Initialize(BaseGraph graph)
		{
			processor = new BaseGraphProcessor(graph);

			Button	b = new Button(OnPlay) { name = "ActionButton", text = "Play !" };

			Add(b);
		}

		void OnPlay()
		{
			processor.ProcessJob();
		}
	}
}
