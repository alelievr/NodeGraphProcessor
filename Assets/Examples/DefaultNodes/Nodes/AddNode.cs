using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using Unity.Jobs;
using Unity.Collections;
using System.Linq;
using NativeCollections;

[System.Serializable, NodeMenuItem("Primitives/Add")]
public class AddNode : BaseNode
{
	[Input("Input", nameof(PullInputFields))]
	public ManagedNativeArray< float >	inputs = new ManagedNativeArray< float >();

	[Output]
	public float				output;

	public override string		name { get { return "Add"; } }

	public int					test;

	struct AddJob : IJob
	{
		public NativeArray< float >	inputs;
		public float				result;

		public void Execute()
		{
			result = 0;

			foreach (float input in inputs)
				result += input;
		}
	}

	protected override JobHandle Schedule(JobHandle dependency)
	{
		AddJob		addJob = new AddJob();
		JobHandle	handle;

		addJob.inputs = inputs.native;

		handle = addJob.Schedule(dependency);
		
		return handle;
	}

	void PullInputFields(IEnumerable< object > values)
	{
		var array = values.Select(o => System.Convert.ToSingle(o)).ToArray();

		inputs.CopyFrom(array);
	}
}
