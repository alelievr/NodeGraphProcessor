using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
	public class BaseGraphZoomer : MouseManipulator
	{
		protected override void RegisterCallbacksOnTarget()
        {
            var baseGraphView = target as BaseGraphView;

            if (baseGraphView == null)
                throw new InvalidOperationException("Manipulator can only be added to a BaseGraphView");

            target.RegisterCallback< WheelEvent, BaseGraphView >(OnSavePan, baseGraphView);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback< WheelEvent, BaseGraphView >(OnSavePan);
        }

		void OnSavePan(WheelEvent e, BaseGraphView graphView)
		{
			//Save graph scale:
			graphView.graph.scale = graphView.viewTransform.scale;
		}

	}
}