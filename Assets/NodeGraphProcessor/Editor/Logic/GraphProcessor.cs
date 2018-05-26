using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;

namespace GraphProcessor
{
	public enum	GraphProcessState
	{
		Playing,
		Paused,
		Stopped,
	}

	class GraphScheduleList
	{
		public BaseNode			node;
		public BaseNode[]		dependencies;

		public GraphScheduleList(BaseNode node)
		{
			this.node = node;
		}
	}

	public class BaseGraphProcessor
	{
		public GraphProcessState	state { get; private set; }
		BaseGraph					graph;
		GraphScheduleList[]			scheduleList;

		struct NodeProcessJob : IJob
		{
			BaseNode node;

			public NodeProcessJob(BaseNode nodeToProcess)
			{
				node = nodeToProcess;
			}

			public void Execute()
			{
				node.OnProcess();
			}
		}

		public BaseGraphProcessor(BaseGraph graph)
		{
			this.graph = graph;

			UpdateComputeOrder();
		}

		void UpdateComputeOrder()
		{
			scheduleList = graph.nodes.OrderBy(n => n.computeOrder).Select(n => {
				GraphScheduleList gsl = new GraphScheduleList(n);
				gsl.dependencies = n.GetInputNodes().ToArray();
				return gsl;
			}).ToArray();
		}

		public virtual void Process()
		{
			int count = scheduleList.Length;
			var scheduledHandles = new Dictionary< BaseNode, JobHandle >();

			for (int i = 0; i < count; i++)
			{
				var schedule = scheduleList[i];
				NodeProcessJob job = new NodeProcessJob();

				scheduledHandles[schedule.node] = job.Schedule();

				int dependenciesCount = schedule.dependencies.Length;
				for (int j = 0; j < dependenciesCount; j++)
					job.Schedule(scheduledHandles[schedule.dependencies[j]]);
			}
		}
	}
}