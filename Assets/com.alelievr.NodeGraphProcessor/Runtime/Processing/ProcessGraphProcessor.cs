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
	public class ProcessGraphProcessor : BaseGraphProcessor
	{
		List< BaseNode >		processList;
		
		/// <summary>
		/// Manage graph scheduling and processing
		/// </summary>
		/// <param name="graph">Graph to be processed</param>
		public ProcessGraphProcessor(BaseGraph graph) : base(graph) {}

		public override void UpdateComputeOrder()
		{
			processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
		}

		/// <summary>
		/// Schedule the graph into the job system
		/// </summary>
		public override void Run()
		{
			int count = processList.Count;

			for (int i = 0; i < count; i++)
			{
				processList[i].OnProcess();
			}

			JobHandle.ScheduleBatchedJobs();
		}
	}
}
