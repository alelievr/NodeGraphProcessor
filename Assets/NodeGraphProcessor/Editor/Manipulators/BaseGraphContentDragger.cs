using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
	public class BaseGraphContentDragger : ContentDragger
	{
		protected override void RegisterCallbacksOnTarget()
        {
            var baseGraphView = target as BaseGraphView;

            if (baseGraphView == null)
                throw new InvalidOperationException("Manipulator can only be added to a BaseGraphView");

			base.RegisterCallbacksOnTarget();

            target.RegisterCallback< MouseUpEvent, BaseGraphView >(OnSavePan, baseGraphView);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback< MouseUpEvent, BaseGraphView >(OnSavePan);
        }

		void OnSavePan(MouseUpEvent e, BaseGraphView graphView)
		{
			//Save graph position:
			graphView.graph.position = graphView.viewTransform.position;
		}

	}
}
