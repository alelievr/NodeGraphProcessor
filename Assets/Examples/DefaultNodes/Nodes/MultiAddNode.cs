using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/MultiAdd")]
public class MultiAddNode : BaseNode
{
	[Input]
	public IEnumerable< float >	inputs = null;

	[Output(allowMultiple = false)]
	public float				output;

	public override string		name => "Add";

	[CustomPortBehavior(nameof(inputs), true)]
	IEnumerable< PortData > ListPortBehavior(List< SerializableEdge > edges)
	{
		for (int i = 0; i < edges.Count + 1; i++)
		{
			yield return new PortData {
				displayName = "In " + i,
				displayType = typeof(float),
				identifier = i.ToString(), // Must be unique
			};
		}
	}

	[CustomPortInput(nameof(inputs), typeof(float), allowCast = true)]
	public void GetInputs(List< SerializableEdge > edges)
	{
		Debug.Log("Edges: " + edges.Count);
		inputs = edges.Select(e => (float)e.passThroughBuffer);
	}

	protected override void Process()
	{
		output = 0;

		if (inputs == null)
			return ;

		foreach (float input in inputs)
			output += input;
	}
}
