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
	[Input(name = "In Values", allowMultiple = true)]
	public ManagedNativeArray< float >	inputs = new ManagedNativeArray< float >();

	[Output]
	public float				output;

	public override string		name => "Add";

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

	[CustomPortInput(nameof(inputs), typeof(float), allowCast = true)]
	public void GetInputs(List< SerializableEdge > edges)
	{
		var array = edges.Select(e => (float)e.passThroughBuffer).ToArray();

		inputs.CopyFrom(array);
	}
}
