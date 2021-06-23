// #define DEBUG_LAMBDA

using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq.Expressions;
using System;

namespace GraphProcessor
{
	/// <summary>
	/// Class that describe port attributes for it's creation
	/// </summary>
	public class PortData : IEquatable< PortData >
	{
		/// <summary>
		/// Unique identifier for the port
		/// </summary>
		public string	identifier;
		/// <summary>
		/// Display name on the node
		/// </summary>
		public string	displayName;
		/// <summary>
		/// The type that will be used for coloring with the type stylesheet
		/// </summary>
		public Type		displayType;
		/// <summary>
		/// If the port accept multiple connection
		/// </summary>
		public bool		acceptMultipleEdges;
		/// <summary>
		/// Port size, will also affect the size of the connected edge
		/// </summary>
		public int		sizeInPixel;
		/// <summary>
		/// Tooltip of the port
		/// </summary>
		public string	tooltip;
		/// <summary>
		/// Is the port vertical
		/// </summary>
		public bool		vertical;

        public bool Equals(PortData other)
        {
			return identifier == other.identifier
				&& displayName == other.displayName
				&& displayType == other.displayType
				&& acceptMultipleEdges == other.acceptMultipleEdges
				&& sizeInPixel == other.sizeInPixel
				&& tooltip == other.tooltip
				&& vertical == other.vertical;
        }

		public void CopyFrom(PortData other)
		{
			identifier = other.identifier;
			displayName = other.displayName;
			displayType = other.displayType;
			acceptMultipleEdges = other.acceptMultipleEdges;
			sizeInPixel = other.sizeInPixel;
			tooltip = other.tooltip;
			vertical = other.vertical;
		}
    }

	/// <summary>
	/// Runtime class that stores all info about one port that is needed for the processing
	/// </summary>
	public class NodePort
	{
		/// <summary>
		/// The actual name of the property behind the port (must be exact, it is used for Reflection)
		/// </summary>
		public string				fieldName;
		/// <summary>
		/// The node on which the port is
		/// </summary>
		public BaseNode				owner;
		/// <summary>
		/// The fieldInfo from the fieldName
		/// </summary>
		public FieldInfo			fieldInfo;
		/// <summary>
		/// Data of the port
		/// </summary>
		public PortData				portData;
		List< SerializableEdge >	edges = new List< SerializableEdge >();
		Dictionary< SerializableEdge, PushDataDelegate >	pushDataDelegates = new Dictionary< SerializableEdge, PushDataDelegate >();
		List< SerializableEdge >	edgeWithRemoteCustomIO = new List< SerializableEdge >();

		/// <summary>
		/// Owner of the FieldInfo, to be used in case of Get/SetValue
		/// </summary>
		public object				fieldOwner;

		CustomPortIODelegate		customPortIOMethod;

		/// <summary>
		/// Delegate that is made to send the data from this port to another port connected through an edge
		/// This is an optimization compared to dynamically setting values using Reflection (which is really slow)
		/// More info: https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/
		/// </summary>
		public delegate void PushDataDelegate();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="owner">owner node</param>
		/// <param name="fieldName">the C# property name</param>
		/// <param name="portData">Data of the port</param>
		public NodePort(BaseNode owner, string fieldName, PortData portData) : this(owner, owner, fieldName, portData) {}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="owner">owner node</param>
		/// <param name="fieldOwner"></param>
		/// <param name="fieldName">the C# property name</param>
		/// <param name="portData">Data of the port</param>
		public NodePort(BaseNode owner, object fieldOwner, string fieldName, PortData portData)
		{
			this.fieldName = fieldName;
			this.owner     = owner;
			this.portData  = portData;
			this.fieldOwner = fieldOwner;

			fieldInfo = fieldOwner.GetType().GetField(
				fieldName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			customPortIOMethod = CustomPortIO.GetCustomPortMethod(owner.GetType(), fieldName);
		}

		/// <summary>
		/// Connect an edge to this port
		/// </summary>
		/// <param name="edge"></param>
		public void Add(SerializableEdge edge)
		{
			if (!edges.Contains(edge))
				edges.Add(edge);

			if (edge.inputNode == owner)
			{
				if (edge.outputPort.customPortIOMethod != null)
					edgeWithRemoteCustomIO.Add(edge);
			}
			else
			{
				if (edge.inputPort.customPortIOMethod != null)
					edgeWithRemoteCustomIO.Add(edge);
			}

			//if we have a custom io implementation, we don't need to genereate the defaut one
			if (edge.inputPort.customPortIOMethod != null || edge.outputPort.customPortIOMethod != null)
				return ;

			PushDataDelegate edgeDelegate = CreatePushDataDelegateForEdge(edge);

			if (edgeDelegate != null)
				pushDataDelegates[edge] = edgeDelegate;
		}

		PushDataDelegate CreatePushDataDelegateForEdge(SerializableEdge edge)
		{
			try
			{
				//Creation of the delegate to move the data from the input node to the output node:
				FieldInfo inputField = edge.inputNode.GetType().GetField(edge.inputFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				FieldInfo outputField = edge.outputNode.GetType().GetField(edge.outputFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				Type inType, outType;

#if DEBUG_LAMBDA
				return new PushDataDelegate(() => {
					var outValue = outputField.GetValue(edge.outputNode);
					inType = edge.inputPort.portData.displayType ?? inputField.FieldType;
					outType = edge.outputPort.portData.displayType ?? outputField.FieldType;
					Debug.Log($"Push: {inType}({outValue}) -> {outType} | {owner.name}");

					object convertedValue = outValue;
					if (TypeAdapter.AreAssignable(outType, inType))
					{
						var convertionMethod = TypeAdapter.GetConvertionMethod(outType, inType);
						Debug.Log("Convertion method: " + convertionMethod.Name);
						convertedValue = convertionMethod.Invoke(null, new object[]{ outValue });
					}

					inputField.SetValue(edge.inputNode, convertedValue);
				});
#endif

// We keep slow checks inside the editor
#if UNITY_EDITOR
				if (!BaseGraph.TypesAreConnectable(inputField.FieldType, outputField.FieldType))
				{
					Debug.LogError("Can't convert from " + inputField.FieldType + " to " + outputField.FieldType + ", you must specify a custom port function (i.e CustomPortInput or CustomPortOutput) for non-implicit convertions");
					return null;
				}
#endif

				Expression inputParamField = Expression.Field(Expression.Constant(edge.inputNode), inputField);
				Expression outputParamField = Expression.Field(Expression.Constant(edge.outputNode), outputField);

				inType = edge.inputPort.portData.displayType ?? inputField.FieldType;
				outType = edge.outputPort.portData.displayType ?? outputField.FieldType;

				// If there is a user defined convertion function, then we call it
				if (TypeAdapter.AreAssignable(outType, inType))
				{
					// We add a cast in case there we're calling the conversion method with a base class parameter (like object)
					var convertedParam = Expression.Convert(outputParamField, outType);
					outputParamField = Expression.Call(TypeAdapter.GetConvertionMethod(outType, inType), convertedParam);
					// In case there is a custom port behavior in the output, then we need to re-cast to the base type because
					// the convertion method return type is not always assignable directly:
					outputParamField = Expression.Convert(outputParamField, inputField.FieldType);
				}
				else // otherwise we cast
					outputParamField = Expression.Convert(outputParamField, inputField.FieldType);

				BinaryExpression assign = Expression.Assign(inputParamField, outputParamField);
				return Expression.Lambda< PushDataDelegate >(assign).Compile();
			} catch (Exception e) {
				Debug.LogError(e);
				return null;
			}
		}

		/// <summary>
		/// Disconnect an Edge from this port
		/// </summary>
		/// <param name="edge"></param>
		public void Remove(SerializableEdge edge)
		{
			if (!edges.Contains(edge))
				return;

			pushDataDelegates.Remove(edge);
			edgeWithRemoteCustomIO.Remove(edge);
			edges.Remove(edge);
		}

		/// <summary>
		/// Get all the edges connected to this port
		/// </summary>
		/// <returns></returns>
		public List< SerializableEdge > GetEdges() => edges;

		/// <summary>
		/// Push the value of the port through the edges
		/// This method can only be called on output ports
		/// </summary>
		public void PushData()
		{
			if (customPortIOMethod != null)
			{
				customPortIOMethod(owner, edges, this);
				return ;
			}

			foreach (var pushDataDelegate in pushDataDelegates)
				pushDataDelegate.Value();

			if (edgeWithRemoteCustomIO.Count == 0)
				return ;

			//if there are custom IO implementation on the other ports, they'll need our value in the passThrough buffer
			object ourValue = fieldInfo.GetValue(fieldOwner);
			foreach (var edge in edgeWithRemoteCustomIO)
				edge.passThroughBuffer = ourValue;
		}

		/// <summary>
		/// Reset the value of the field to default if possible
		/// </summary>
		public void ResetToDefault()
		{
			// Clear lists, set classes to null and struct to default value.
			if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
				(fieldInfo.GetValue(fieldOwner) as IList)?.Clear();
			else if (fieldInfo.FieldType.GetTypeInfo().IsClass)
				fieldInfo.SetValue(fieldOwner, null);
			else
			{
				try
				{
					fieldInfo.SetValue(fieldOwner, Activator.CreateInstance(fieldInfo.FieldType));
				} catch {} // Catch types that don't have any constructors
			}
		}

		/// <summary>
		/// Pull values from the edge (in case of a custom convertion method)
		/// This method can only be called on input ports
		/// </summary>
		public void PullData()
		{
			if (customPortIOMethod != null)
			{
				customPortIOMethod(owner, edges, this);
				return ;
			}

			// check if this port have connection to ports that have custom output functions
			if (edgeWithRemoteCustomIO.Count == 0)
				return ;

			// Only one input connection is handled by this code, if you want to
			// take multiple inputs, you must create a custom input function see CustomPortsNode.cs
			if (edges.Count > 0)
			{
				var passThroughObject = edges.First().passThroughBuffer;

				// We do an extra convertion step in case the buffer output is not compatible with the input port
				if (passThroughObject != null)
					if (TypeAdapter.AreAssignable(fieldInfo.FieldType, passThroughObject.GetType()))
						passThroughObject = TypeAdapter.Convert(passThroughObject, fieldInfo.FieldType);

				fieldInfo.SetValue(fieldOwner, passThroughObject);
			}
		}
	}

	/// <summary>
	/// Container of ports and the edges connected to these ports
	/// </summary>
	public abstract class NodePortContainer : List< NodePort >
	{
		protected BaseNode node;

		public NodePortContainer(BaseNode node)
		{
			this.node = node;
		}

		/// <summary>
		/// Remove an edge that is connected to one of the node in the container
		/// </summary>
		/// <param name="edge"></param>
		public void Remove(SerializableEdge edge)
		{
			ForEach(p => p.Remove(edge));
		}

		/// <summary>
		/// Add an edge that is connected to one of the node in the container
		/// </summary>
		/// <param name="edge"></param>
		public void Add(SerializableEdge edge)
		{
			string portFieldName = (edge.inputNode == node) ? edge.inputFieldName : edge.outputFieldName;
			string portIdentifier = (edge.inputNode == node) ? edge.inputPortIdentifier : edge.outputPortIdentifier;

			// Force empty string to null since portIdentifier is a serialized value
			if (String.IsNullOrEmpty(portIdentifier))
				portIdentifier = null;

			var port = this.FirstOrDefault(p =>
			{
				return p.fieldName == portFieldName && p.portData.identifier == portIdentifier;
			});

			if (port == null)
			{
				Debug.LogError("The edge can't be properly connected because it's ports can't be found");
				return;
			}

			port.Add(edge);
		}
	}

	/// <inheritdoc/>
	public class NodeInputPortContainer : NodePortContainer
	{
		public NodeInputPortContainer(BaseNode node) : base(node) {}

		public void PullDatas()
		{
			ForEach(p => p.PullData());
		}
	}

	/// <inheritdoc/>
	public class NodeOutputPortContainer : NodePortContainer
	{
		public NodeOutputPortContainer(BaseNode node) : base(node) {}

		public void PushDatas()
		{
			ForEach(p => p.PushData());
		}
	}
}