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

		public InputAttribute(string name = "")
		{
			this.name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class OutputAttribute : Attribute
	{
		public string		name;

		public OutputAttribute(string name = "")
		{
			this.name = name;
		}
	}
}