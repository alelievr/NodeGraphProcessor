using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace GraphProcessor
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class InputAttribute : Attribute
	{
		public string		name;
		public bool			allowMultiple = false;
		
		public InputAttribute(string name = null, bool allowMultiple = false)
		{
			this.name = name;
			this.allowMultiple = allowMultiple;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class OutputAttribute : Attribute
	{
		public string		name;

		public OutputAttribute(string name = null)
		{
			this.name = name;
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
}