using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
	public class MiniMapView : MiniMap
	{
		BaseGraphView	graphView;
		Vector2			size;

		public MiniMapView(BaseGraphView baseGraphView)
		{
			this.graphView = baseGraphView;
			SetPosition(new Rect(0, 0, 100, 100));
			size = new Vector2(100, 100);
		}
	}
}