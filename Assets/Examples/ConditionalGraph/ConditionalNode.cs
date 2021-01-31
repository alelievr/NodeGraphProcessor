using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using UnityEngine;

namespace NodeGraphProcessor.Examples
{
	[System.Serializable]
	/// <summary>
	/// This is the base class for every node that is executed by the conditional processor, it takes an executed bool as input to 
	/// </summary>
	public abstract class ConditionalNode : BaseNode, IConditionalNode
	{
		// These booleans will controls wether or not the execution of the folowing nodes will be done or discarded.
		[Input(name = "Executed", allowMultiple = true)]
		public ConditionalLink	executed;

		public abstract IEnumerable< ConditionalNode >	GetExecutedNodes();

		// Assure that the executed field is always at the top of the node port section
		public override FieldInfo[] GetNodeFields()
		{
			var fields = base.GetNodeFields();
			Array.Sort(fields, (f1, f2) => f1.Name == nameof(executed) ? -1 : 1);
			return fields;
		}
	}

	[System.Serializable]
	/// <summary>
	/// This class represent a simple node which takes one event in parameter and pass it to the next node
	/// </summary>
	public abstract class LinearConditionalNode : ConditionalNode, IConditionalNode
	{
		[Output(name = "Executes")]
		public ConditionalLink	executes;

		public override IEnumerable< ConditionalNode >	GetExecutedNodes()
		{
			// Return all the nodes connected to the executes port
			return outputPorts.FirstOrDefault(n => n.fieldName == nameof(executes))
				.GetEdges().Select(e => e.inputNode as ConditionalNode);
		}
	}
	
	[System.Serializable]
	/// <summary>
	/// This class represent a waitable node which invokes another node after a time/frame
	/// </summary>
	public abstract class WaitableNode : LinearConditionalNode
	{
		[Output(name = "Execute After")]
		public ConditionalLink executeAfter;

		protected void ProcessFinished()
		{
			onProcessFinished.Invoke(this);
		}

		[HideInInspector]
		public Action<WaitableNode> onProcessFinished;

		public IEnumerable< ConditionalNode > GetExecuteAfterNodes()
		{
			return outputPorts.FirstOrDefault(n => n.fieldName == nameof(executeAfter))
			                  .GetEdges().Select(e => e.inputNode as ConditionalNode);
		}
	}
}