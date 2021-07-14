using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(AbstractNode))]
public class AbstractNodeView : BaseNodeView
{
	public override void Enable()
	{
		controlsContainer.Add(new Label("Inheritance support"));
	}
}