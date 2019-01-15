using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
	public class BlackboardView : Blackboard
	{
		BaseGraphView	graphView;
        BaseGraph       graph;
		Vector2			size;

		public BlackboardView(BaseGraphView baseGraphView)
		{
			this.graphView = baseGraphView;
            this.graph = graphView.graph;
			SetPosition(new Rect(0, 0, 100, 100));
			size = new Vector2(100, 100);

            addItemRequested = OnAddItem;
        }

        void OnAddItem(Blackboard b)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(EditorGUIUtility.TrTextContent("Category"), false, () => { });
            menu.AddSeparator(string.Empty);

            // foreach (var parameter in VFXLibrary.GetParameters())
            // {
            //     menu.AddItem(EditorGUIUtility.TextContent(type.UserFriendlyName()), false, OnAddParameter, parameter);
            // }

            menu.ShowAsContext();
        }
	}
}