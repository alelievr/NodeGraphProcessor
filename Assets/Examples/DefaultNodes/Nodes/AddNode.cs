using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using Unity.Jobs;
using Unity.Collections;

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
		inputs = new NativeArray<float>();
	}
}
