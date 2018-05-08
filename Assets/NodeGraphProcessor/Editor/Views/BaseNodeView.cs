using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Rendering;
using UnityEditor;

using NodeView = UnityEditor.Experimental.UIElements.GraphView.Node;

namespace GraphProcessor
{
	[NodeCustomEditor(typeof(BaseNode))]
	public class BaseNodeView : NodeView
	{
		protected BaseNode	nodeTarget;

        protected VisualElement controlsContainer;

		public void Initialize(BaseNode node)
		{
			nodeTarget = node;
			
			AddStyleSheetPath("Styles/BaseNodeView");
			// AddToClassList("Node");
			
            controlsContainer = new VisualElement { name = "controls" };
            {
                var m_ControlsDivider = new VisualElement { name = "divider" };
                m_ControlsDivider.AddToClassList("horizontal");
                controlsContainer.Add(m_ControlsDivider);
                var m_ControlItems = new VisualElement { name = "items" };
                controlsContainer.Add(m_ControlItems);
            }
        	mainContainer.Add(controlsContainer);

			Enable();
		}

		public virtual void Enable()
		{
			var field = new TextField();
			mainContainer.Add(field);
			//TODO: draw custom inspector with reflection
		}
	}
}