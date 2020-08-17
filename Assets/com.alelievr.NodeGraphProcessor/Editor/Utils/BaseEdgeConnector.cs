using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;

namespace GraphProcessor
{
	public class BaseEdgeConnector : EdgeConnector
	{
		BaseEdgeDragHelper dragHelper;
        Edge m_EdgeCandidate;
        private bool m_Active;
        Vector2 m_MouseDownPosition;

        internal const float k_ConnectionDistanceTreshold = 10f;

		public BaseEdgeConnector(IEdgeConnectorListener listener) : base()
		{
            dragHelper = new BaseEdgeDragHelper(listener);
            m_Active = false;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
		}

		public override EdgeDragHelper edgeDragHelper
		{
			get
			{
				return dragHelper;
			}
		}

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<MouseCaptureOutEvent>(OnCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        protected virtual void OnMouseDown(MouseDownEvent e)
        {
            if (m_Active)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (!CanStartManipulation(e))
            {
                return;
            }

            var graphElement = target as Port;
            if (graphElement == null)
            {
                return;
            }

            m_MouseDownPosition = e.localMousePosition;

            m_EdgeCandidate = new EdgeView();
            edgeDragHelper.draggedPort = graphElement;
            edgeDragHelper.edgeCandidate = m_EdgeCandidate;

            if (edgeDragHelper.HandleMouseDown(e))
            {
                m_Active = true;
                target.CaptureMouse();

                e.StopPropagation();
            }
            else
            {
                edgeDragHelper.Reset();
                m_EdgeCandidate = null;
            }
        }

        void OnCaptureOut(MouseCaptureOutEvent e)
        {
            m_Active = false;
            if (m_EdgeCandidate != null)
                Abort();
        }

        protected virtual void OnMouseMove(MouseMoveEvent e)
        {
            if (!m_Active) return;

            edgeDragHelper.HandleMouseMove(e);
            m_EdgeCandidate.candidatePosition = e.mousePosition;
            m_EdgeCandidate.UpdateEdgeControl();
            e.StopPropagation();
        }

        protected virtual void OnMouseUp(MouseUpEvent e)
        {
            if (!m_Active || !CanStopManipulation(e))
                return;

            if (CanPerformConnection(e.localMousePosition))
                edgeDragHelper.HandleMouseUp(e);
            else
                Abort();

            m_Active = false;
            m_EdgeCandidate = null;
            target.ReleaseMouse();
            e.StopPropagation();
        }

        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode != KeyCode.Escape || !m_Active)
                return;

            Abort();

            m_Active = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }

        void Abort()
        {
            var graphView = target?.GetFirstAncestorOfType<GraphView>();
            graphView?.RemoveElement(m_EdgeCandidate);

            m_EdgeCandidate.input = null;
            m_EdgeCandidate.output = null;
            m_EdgeCandidate = null;

            edgeDragHelper.Reset();
        }

        bool CanPerformConnection(Vector2 mousePosition)
        {
            return Vector2.Distance(m_MouseDownPosition, mousePosition) > k_ConnectionDistanceTreshold;
        }
    }
}