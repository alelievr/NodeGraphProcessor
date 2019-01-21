using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using System.Linq;

namespace GraphProcessor
{
	public class BlackboardFieldView : BlackboardField
	{
		protected BaseGraphView	graphView;

		public BlackboardFieldView(string name) : base(null, name, null)
		{
        }
	}
}