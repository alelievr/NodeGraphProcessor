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
	public PortArray< float >	inputs = new PortArray< float >();

	[Output]
	public float				output;

	public override string		name { get { return "Add"; } }

	public int					test;

	struct AddJob : IJob
	{
		NativeArray< float >	inputs;
		float					result;

		public void Execute()
		{
			result = 0;

			foreach (float input in inputs)
				result += input;
		}
	}

	public override JobHandle Schedule(JobHandle[] dependencies)
	{
		AddJob		addJob = new AddJob();
		JobHandle	handle;
		
		//Will this work ?
		handle = addJob.Schedule();

		foreach (JobHandle dep in dependencies)
			addJob.Schedule(dep);
		
		return handle;
	}

	void PullInputFields(IEnumerable< object > values)
	{
		foreach (var value in values)
			inputs.Add((float)value);
	}
}
