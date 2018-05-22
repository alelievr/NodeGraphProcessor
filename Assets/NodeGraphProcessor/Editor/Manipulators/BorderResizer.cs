using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

namespace GraphProcessor
{
    public class BorderResizer : MouseManipulator
    {
        bool                active;
        Vector2             startMousePosition;
        Vector2             startComponentSize;
        Vector2             startComponentPosition;

        readonly int        dragBorderSize = 15;
        Vector2             dragDirection;

        readonly string     cursorBorderStyleSheet = "GraphProcessorStyles/BorderResizer";

        GraphElement        elem;

        public BorderResizer()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            elem = target as GraphElement;

            if (elem == null)
                throw new System.InvalidOperationException("BorderReiszer can only be added to a GraphElement");
            
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

            if (!IsMouseOverBorders(e.localMousePosition))
                return;

            if (CanStartManipulation(e))
            {
                active = true;
                target.TakeMouseCapture();
                e.StopPropagation();

                startComponentSize = new Vector2(elem.style.width, elem.style.height);
                startMousePosition = e.localMousePosition;
                startComponentPosition = elem.transform.position;
            }
            else
                Debug.Log("can't start manipulation !");
        }

        void OnMouseMove(MouseMoveEvent e)
        {
            if (!active)
                return ;

            Vector2 delta = e.localMousePosition - startMousePosition + (Vector2)elem.transform.position - startComponentPosition;

            elem.style.width = startComponentSize.x + delta.x * dragDirection.x;
            elem.style.height = startComponentSize.y + delta.y * dragDirection.y;

            elem.transform.position -= (Vector3)(e.mouseDelta * Vector2.Min(Vector2.zero, dragDirection));
        }

        void OnMouseUp(MouseUpEvent e)
        {
            if (!active)
                return ;

            if (CanStopManipulation(e))
            {
                target.ReleaseMouseCapture();
                e.StopPropagation();

                GraphView graphView = elem.GetFirstAncestorOfType<GraphView>();
                if (graphView != null && graphView.elementResized != null)
                    graphView.elementResized(elem);
            }

            active = false;
        }

        bool IsMouseOverBorders(Vector2 mousePosition)
        {
            Rect borders = new Rect(Vector2.zero, target.localBound.size);

            dragDirection = Vector2.zero;

            if (mousePosition.x - borders.xMin < dragBorderSize)
                dragDirection.x = -1;
            if (borders.xMax - mousePosition.x < dragBorderSize)
                dragDirection.x = 1;
            if (mousePosition.y - borders.yMin < dragBorderSize)
                dragDirection.y = -1;
            if (borders.yMax - mousePosition.y < dragBorderSize)
                dragDirection.y = 1;

            return dragDirection != Vector2.zero;
        }
    }
}