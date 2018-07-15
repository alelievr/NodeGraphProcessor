using System;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;

using UnityEditor.ShaderGraph.Drawing;

namespace GraphProcessor
{
    public class ConfinedDragger : MouseManipulator
    {
        bool active;

		VisualElement container;
        VisualElement handle;

		public Action onDragEnd;

        public ConfinedDragger(VisualElement container)
        {
			this.container = container;
            active = false;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            handle = target;
			
            handle.RegisterCallback(new EventCallback<MouseDownEvent>(OnMouseDown), Capture.NoCapture);
            handle.RegisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove), Capture.NoCapture);
            handle.RegisterCallback(new EventCallback<MouseUpEvent>(OnMouseUp), Capture.NoCapture);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            handle.UnregisterCallback(new EventCallback<MouseDownEvent>(OnMouseDown), Capture.NoCapture);
            handle.UnregisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove), Capture.NoCapture);
            handle.UnregisterCallback(new EventCallback<MouseUpEvent>(OnMouseUp), Capture.NoCapture);
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            active = true;

            handle.TakeMouseCapture();
            evt.StopImmediatePropagation();
        }

        void OnMouseMove(MouseMoveEvent evt)
        {
            if (active)
            {
				Vector3 position = target.transform.position;
				position += (Vector3)evt.mouseDelta;
                position.x = Mathf.Clamp(position.x, -target.layout.position.x, container.layout.width - target.layout.position.x - target.localBound.size.x);
                position.y = Mathf.Clamp(position.y, -target.layout.position.y, container.layout.height - target.layout.position.y - target.localBound.size.y);
				
				target.transform.position = position;
            }
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            active = false;

            if (handle.HasMouseCapture())
                handle.ReleaseMouseCapture();
            evt.StopImmediatePropagation();

			if (onDragEnd != null)
				onDragEnd();
        }
    }
}
