﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

[System.Serializable, NodeMenuItem("Custom/Relay")]
public class RelayNode : BaseNode
{
	public struct PackedRelayData
	{
		public List<object>	values;
		public List<string>	names;
		public List<Type>	types;
	}

	[Input(name = "In")]
    public PackedRelayData	input;

	[Output(name = "Out")]
	public PackedRelayData	output;

	public bool		unpackOutput = false;
	public int		inputEdgeCount = 0;
	[System.NonSerialized]
	int				outputIndex = 0;

	const int		k_MaxPortSize = 14;

	protected override void Enable()
	{
		onAfterEdgeConnected += EdgeConnected;
		onAfterEdgeDisconnected += EdgeDisconnected;
	}

	protected override void Process()
	{
		outputIndex = 0;
		output = input;
	}

	public override string layoutStyle => "GraphProcessorStyles/RelayNode";

	void EdgeConnected(SerializableEdge edge)
	{
		UpdateAllPorts();
	}

	void EdgeDisconnected(SerializableEdge edge)
	{
		// If there is still an edge connected to the node then, we do nothing
		if (inputPorts.Any(n => n.GetEdges().Count != 0) || outputPorts.Any(n => n.GetEdges().Count != 0))
			return;
	}

	[CustomPortInput(nameof(input), typeof(object), true)]
	public void GetInputs(List< SerializableEdge > edges)
	{
		inputEdgeCount = edges.Count;
		UpdateAllPorts();

		// If the relay is only connected to another relay:
		if (edges.Count == 1 && edges.First().outputNode.GetType() == typeof(RelayNode))
		{
			input = (PackedRelayData)edges.First().passThroughBuffer;
		}
		else
		{
			input.values = edges.Select(e => e.passThroughBuffer).ToList();
			input.names = edges.Select(e => e.outputPort.portData.displayName).ToList();
			input.types = edges.Select(e => e.outputPort.portData.displayType ?? e.outputPort.fieldInfo.FieldType).ToList();
		}
	}

	[CustomPortOutput(nameof(output), typeof(object), true)]
	public void PushOutputs(List< SerializableEdge > edges)
	{
		// In case relay is not connected
		if (output.values == null || output.values.Count == 0)
			return;

		var inputPortEdges = inputPorts[0].GetEdges();

		if (unpackOutput && inputPortEdges.Count == 1)
		{
			// When we unpack the output, there is one port per type of data in output
			// That means that this function will be called the same number of time than the output port count
			// Thus we use a class field to keep the index.
			object data = output.values[outputIndex++];

			foreach (var edge in edges)
				edge.passThroughBuffer = data;
		}
		else
		{
			foreach (var edge in edges)
				edge.passThroughBuffer = output;
		}
	}

	[CustomPortBehavior(nameof(input))]
	IEnumerable< PortData > InputPortBehavior(List< SerializableEdge > edges)
	{
		// When the node is initialized, the input ports is empty because it's this function that generate the ports
		int sizeInPixel = 0;
		if (inputPorts.Count != 0)
		{
			// Add the size of all input edges:
			var inputEdges = inputPorts[0]?.GetEdges();
			sizeInPixel = inputEdges.Sum(e => Mathf.Max(0, e.outputPort.portData.sizeInPixel - 8));
		}

		yield return new PortData {
			displayName = "",
			displayType = typeof(object),
			identifier = "0",
			acceptMultipleEdges = true,
			sizeInPixel = Mathf.Min(k_MaxPortSize, sizeInPixel + 8),
		};
	}

	[CustomPortBehavior(nameof(output))]
	IEnumerable< PortData > OutputPortBehavior(List< SerializableEdge > edges)
	{
		var inputPortEdges = inputPorts[0].GetEdges();
		var underlyingPortData = GetUnderlyingPortDataList();
		if (unpackOutput && inputPortEdges.Count == 1)
		{
			for (int i = 0; i < underlyingPortData.Count; i++)
			{
				yield return new PortData {
					displayName = underlyingPortData?[i].name ?? "",
					displayType = underlyingPortData?[i].type ?? typeof(Texture),
					identifier = i.ToString(),
					acceptMultipleEdges = true,
					sizeInPixel = 0,
				};
			}
		}
		else
		{
			yield return new PortData {
				displayName = "",
				displayType = typeof(object),
				identifier = "0",
				acceptMultipleEdges = true,
				sizeInPixel = Mathf.Min(k_MaxPortSize, Mathf.Max(underlyingPortData.Count, 1) + 7),
			};
		}
	}

	static List<(Type, string)> s_empty = new List<(Type, string)>();
	public List<(Type type, string name)> GetUnderlyingPortDataList()
	{
		// get input edges:
		if (inputPorts.Count == 0)
			return s_empty;

		var inputEdges = GetNonRelayEdges();

		if (inputEdges != null)
			return inputEdges.Select(e => (e.outputPort.portData.displayType ?? e.outputPort.fieldInfo.FieldType, e.outputPort.portData.displayName)).ToList();

		return s_empty;
	}

	public List<SerializableEdge> GetNonRelayEdges()
	{
		var inputEdges = inputPorts?[0]?.GetEdges();

		// Iterate until we don't have a relay node in input
		while (inputEdges.Count == 1 && inputEdges.First().outputNode.GetType() == typeof(RelayNode))
			inputEdges = inputEdges.First().outputNode.inputPorts[0]?.GetEdges();

		return inputEdges;
	}
}