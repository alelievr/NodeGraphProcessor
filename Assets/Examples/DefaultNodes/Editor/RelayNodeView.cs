using UnityEngine;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.Experimental.GraphView;

[NodeCustomEditor(typeof(RelayNode))]
public class RelayNodeView : BaseNodeView
{
	RelayNode	relay => nodeTarget as RelayNode;

	public override void Enable()
	{
		// Remove useless elements
		this.Q("title").RemoveFromHierarchy();
		this.Q("divider").RemoveFromHierarchy();

		onPortConnected += (e) => UpdatePortTypes(e);
		onPortDisconnected += (e) => UpdatePortTypes(e, typeof(object));

		ForceUpdatePorts();
	}

	public override void SetPosition(Rect newPos)
	{
		base.SetPosition(new Rect(newPos.position, new Vector2(200, 200)));
		style.height = 30;
		style.width = 60;
	}

	void UpdatePortTypes(PortView portView, Type forceType = null)
	{
		schedule.Execute(() => {
			ForceUpdatePorts();
			owner.nodeViews.ForEach(n => {
				if (n is RelayNodeView) n.MarkDirtyRepaint();
			});
			portView.GetEdges().ForEach(e => e.MarkDirtyRepaint());
		}).ExecuteLater(1);
	}
}