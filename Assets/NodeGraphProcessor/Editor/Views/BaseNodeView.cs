using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Rendering;
using UnityEditor;

using NodeView = UnityEditor.Experimental.UIElements.GraphView.Node;

namespace GraphProcessor
{
	[CustomEditor(typeof(BaseNode))]
	public class BaseNodeView : NodeView
	{
		protected BaseNode	nodeTarget;

		public void Initialize(BaseNode node)
		{
			nodeTarget = node;
			
			AddStyleSheetPath("Styles/MaterialNodeView");
			AddToClassList("MaterialNode");

			Enable();
		}

		public virtual void Enable()
		{
			//TODO: draw custom inspector with reflection
		}
	}
}