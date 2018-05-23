using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Jobs;

namespace GraphProcessor
{
	public enum	GraphProcessState
	{
		Playing,
		Paused,
		Stopped,
	}

	public class BaseGraphProcessor
	{
		public GraphProcessState	state { get; private set; }

		BaseGraph					graph;

		BaseNode[]					processOrderedNodes;

		public BaseGraphProcessor(BaseGraph graph)
		{
			this.graph = graph;

			UpdateComputeOrder();
			
		}

		void UpdateComputeOrder()
		{
			var tmp = new List< BaseNode >();

			tmp.AddRange(graph.nodes);
			tmp.Sort((n1, n2) => n1.computeOrder.CompareTo(n2.computeOrder));

			processOrderedNodes = tmp.ToArray();
		}

		public void Process()
		{
			int count = processOrderedNodes.Length;

			for (int i = 0; i < count; i++)
				processOrderedNodes[i].OnProcess();
		}

		public void Pause()
		{

		}

		public void Stop()
		{

		}

		public void NextStep()
		{

		}
	}
}