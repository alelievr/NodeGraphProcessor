using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Rendering;

using NodeView = UnityEditor.Experimental.UIElements.GraphView.Node;

public class BasicNodeView : NodeView
{
	public void Initialize()
	{
		SetSize(new Vector2(200, 100));
	}
}
