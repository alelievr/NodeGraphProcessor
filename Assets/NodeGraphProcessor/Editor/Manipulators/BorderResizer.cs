using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using System;

namespace GraphProcessor
{
    public class BorderResizer : MouseManipulator
    {
        bool active;

        readonly string     cursorBorderStyleSheet = "GraphProcessorStyles/BorderResizer";

        public BorderResizer()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback< MouseDownEvent >(OnMouseDown);
            target.RegisterCallback< MouseMoveEvent >(OnMouseMove);
            target.RegisterCallback< MouseUpEvent >(OnMouseUp);

            if (!target.HasStyleSheetPath(cursorBorderStyleSheet))
                target.AddStyleSheetPath(cursorBorderStyleSheet);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback< MouseDownEvent >(OnMouseDown);
            target.UnregisterCallback< MouseMoveEvent >(OnMouseMove);
            target.UnregisterCallback< MouseUpEvent >(OnMouseUp);
        }

        void OnMouseDown(MouseDownEvent e)
        {
            if (active)
            {
                e.StopPropagation();
                return ;
            }

            if (MouseCaptureController.IsMouseCaptureTaken())
                return ;

            var graphElement = e.target as GraphElement;

            if (graphElement == null)
                return ;

            if (CanStartManipulation(e))
            {
                active = true;
                target.TakeMouseCapture();
                e.StopPropagation();
            }
        }

        void OnMouseMove(MouseMoveEvent e)
        {
            if (!active)
                return ;

            var graphElement = e.target as GraphElement;

            graphElement.style.width += e.mouseDelta.x;
            graphElement.style.height += e.mouseDelta.y;
        }

        void OnMouseUp(MouseUpEvent e)
        {
            if (!active)
                return ;

            var graphElement = target as GraphElement;

            if (CanStopManipulation(e))
            {
                target.ReleaseMouseCapture();
                e.StopPropagation();

                GraphView graphView = graphElement.GetFirstAncestorOfType<GraphView>();
                if (graphView != null && graphView.elementResized != null)
                    graphView.elementResized(graphElement);
            }

            active = false;
        }
    }
}