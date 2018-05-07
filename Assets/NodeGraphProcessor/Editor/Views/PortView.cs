using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using System;

namespace GraphProcessor
{
	public class PortView : Port
	{
		public PortView(Orientation portOrientation, Direction portDirection, Type type) : base(portOrientation, portDirection, type)
		{
			AddStyleSheetPath("Styles/ShaderPort");
		}
	}
}