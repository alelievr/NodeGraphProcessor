using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

namespace GraphProcessor
{
    public class BorderResizer : MouseManipulator
    {
        bool        active;
        Vector2     startMousePosition;
        Vector2     startComponentSize;

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

                startComponentSize = new Vector2(graphElement.style.width, graphElement.style.height);
                startMousePosition = e.localMousePosition;
            }
        }

        void OnMouseMove(MouseMoveEvent e)
        {
            if (!active)
                return ;

            var graphElement = e.target as GraphElement;

            Vector2 delta = e.localMousePosition - startMousePosition;

            graphElement.style.width = startComponentSize.x + delta.x;
            graphElement.style.height = startComponentSize.y + delta.y;
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