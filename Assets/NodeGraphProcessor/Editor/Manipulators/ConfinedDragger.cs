using System;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;

namespace GraphProcessor
{
    public class ConfinedDragger : MouseManipulator
    {
        bool    active;
        Vector3 offset;

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
			
            handle.RegisterCallback(new EventCallback<MouseDownEvent>(OnMouseDown), TrickleDown.NoTrickleDown);
            handle.RegisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove), TrickleDown.NoTrickleDown);
            handle.RegisterCallback(new EventCallback<MouseUpEvent>(OnMouseUp), TrickleDown.NoTrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            handle.UnregisterCallback(new EventCallback<MouseDownEvent>(OnMouseDown), TrickleDown.NoTrickleDown);
            handle.UnregisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove), TrickleDown.NoTrickleDown);
            handle.UnregisterCallback(new EventCallback<MouseUpEvent>(OnMouseUp), TrickleDown.NoTrickleDown);
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            active = true;

            offset = evt.mousePosition - (Vector2)target.transform.position;

            handle.CaptureMouse();
            evt.StopImmediatePropagation();
        }

        void OnMouseMove(MouseMoveEvent evt)
        {
            if (active)
            {
				Vector3 position = (Vector3)evt.mousePosition - offset;
                position.x = Mathf.Clamp(position.x, -target.layout.position.x, container.layout.width - target.layout.position.x - target.localBound.size.x);
                position.y = Mathf.Clamp(position.y, -target.layout.position.y, container.layout.height - target.layout.position.y - target.localBound.size.y);
				
				target.transform.position = position;
            }
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            active = false;

            if (handle.HasMouseCapture())
                handle.ReleaseMouse();
            evt.StopImmediatePropagation();

			if (onDragEnd != null)
				onDragEnd();
        }
    }
}
