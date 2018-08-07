using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
// using Unity.Entities;

namespace GraphProcessor
{

	/// <summary>
	/// Graph processor
	/// </summary>
	public class JobGraphProcessor : BaseGraphProcessor
	{
		GraphScheduleList[]			scheduleList;
		
		internal class GraphScheduleList
		{
			public BaseNode			node;
			public BaseNode[]		dependencies;
	
			public GraphScheduleList(BaseNode node)
			{
				this.node = node;
			}
		}

		/// <summary>
		/// Manage graph scheduling and processing
		/// </summary>
		/// <param name="graph">Graph to be processed</param>
		public JobGraphProcessor(BaseGraph graph) : base(graph) {}

		public override void UpdateComputeOrder()
		{
			scheduleList = graph.nodes.OrderBy(n => n.computeOrder).Select(n => {
				GraphScheduleList gsl = new GraphScheduleList(n);
				gsl.dependencies = n.GetInputNodes().ToArray();
				return gsl;
			}).ToArray();
		}

		/// <summary>
		/// Schedule the graph into the job system
		/// </summary>
		public override void Run()
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

				// TODO: call the onSchedule on the current node
				// JobHandle currentJob = schedule.node.OnSchedule(dep);
				// scheduledHandles[schedule.node] = currentJob;
			}

			JobHandle.ScheduleBatchedJobs();
		}
	}
}
