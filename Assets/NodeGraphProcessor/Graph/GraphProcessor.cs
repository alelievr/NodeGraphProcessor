using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
// using Unity.Entities;

namespace GraphProcessor
{
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
		BaseGraph					graph;
		GraphScheduleList[]			scheduleList;

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

		public void ScheduleJobs()
		{
			int count = scheduleList.Length;
			var scheduledHandles = new Dictionary< BaseNode, JobHandle >();

			for (int i = 0; i < count; i++)
			{
				JobHandle dep = default(JobHandle);
				var schedule = scheduleList[i];
				int dependenciesCount = schedule.dependencies.Length;

				for (int j = 0; j < dependenciesCount; j++)
					dep = JobHandle.CombineDependencies(dep, scheduledHandles[schedule.dependencies[j]]);

				JobHandle currentJob = schedule.node.OnSchedule(dep);

				scheduledHandles[schedule.node] = currentJob;

			}

			JobHandle.ScheduleBatchedJobs();
		}
	}
}
