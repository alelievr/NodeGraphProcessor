using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class FieldDrawerAttribute : Attribute
	{
		public Type		fieldType;

		public FieldDrawerAttribute(Type fieldType)
		{
			this.fieldType = fieldType;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomPortInputAttribute : Attribute
	{
		public string	fieldName;
		public Type		inputType;
		public bool		allowCast;

		public CustomPortInputAttribute(string fieldName, Type inputType, bool allowCast = true)
		{
			this.fieldName = fieldName;
			this.inputType = inputType;
			this.allowCast = allowCast;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomPortOutputAttribute : Attribute
	{
		public string	fieldName;
		public Type		outputType;
		public bool		allowCast;

		public CustomPortOutputAttribute(string fieldName, Type outputType, bool allowCast = true)
		{
			this.fieldName = fieldName;
			this.outputType = outputType;
			this.allowCast = allowCast;
		}
	}

	// Note: The attached function must have this prototype:
	// IEnumerable< PortData > MyCustomPortFunction(List< SerializableEdge > edges);
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomPortBehaviorAttribute : Attribute
	{
		public string		fieldName;

		public CustomPortBehaviorAttribute(string fieldName)
		{
			this.fieldName = fieldName;
		}
	}
}