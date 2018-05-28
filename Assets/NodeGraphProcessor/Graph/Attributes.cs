using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace GraphProcessor
{
	public delegate void InputPullerDelegate(object value);
	public delegate void MultiInputPullerDelegate(IEnumerable< object > value);

	public delegate object OutputPusherDelegate();

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class InputAttribute : Attribute
	{
		public string		name;
		public string		customPullerName;
		
		public InputAttribute(string name = null, string customInputPuller = null)
		{
			this.name = name;
			this.customPullerName = customInputPuller;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class OutputAttribute : Attribute
	{
		public string		name;
		public string		customPusherName;

		public OutputAttribute(string name = null, string customOutputPusher = null)
		{
			this.name = name;
			this.customPusherName = customOutputPusher;
		}
	}
	
	[AttributeUsage(AttributeTargets.Class)]
	public class NodeMenuItemAttribute : Attribute
	{
		public string	menuTitle;

		public NodeMenuItemAttribute(string menuTitle = null)
		{
			this.menuTitle = menuTitle;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class FieldDrawerAttribute : Attribute
	{
		public Type		fieldType;

		public FieldDrawerAttribute(Type fieldType)
		{
			this.fieldType = fieldType;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomDataPush : Attribute
	{
		public Type		dataType;

		public CustomDataPush(Type dataType)
		{
			this.dataType = dataType;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomDataPull : Attribute
	{
		public Type		dataType;

		public CustomDataPull(Type dataType)
		{
			this.dataType = dataType;
		}
	}
}