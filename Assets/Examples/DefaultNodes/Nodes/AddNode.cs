using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using Unity.Jobs;
using Unity.Collections;
using System.Linq;

[System.Serializable, NodeMenuItem("Primitives/Add")]
public class AddNode : BaseNode
{
	[Input("Input", nameof(PullInputFields))]
	public NativeArray< float >	inputs = new NativeArray< float >();

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

	protected override void Enable()
	{
		inputs = new NativeArray<float>();
	}

	protected override void Disable()
	{
		inputs.Dispose();
	}

	protected override JobHandle Schedule(JobHandle dependency)
	{
		AddJob		addJob = new AddJob();
		JobHandle	handle;

		addJob.inputs = new NativeArray<float>(inputs.Length, Allocator.Temp);

		handle = addJob.Schedule(dependency);
		
		return handle;
	}

	void PullInputFields(IEnumerable< object > values)
	{
		var array = values.Cast< float >().ToArray();

		if (inputs.Length != array.Length)
		{
			inputs.Dispose();
			inputs = new NativeArray<float>(array.Length, Allocator.Persistent);
		}

		inputs.CopyFrom(array);
	}
}
