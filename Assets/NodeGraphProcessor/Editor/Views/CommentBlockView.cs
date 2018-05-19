using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Graphs;

namespace GraphProcessor
{
	public class CommentBlockView : GraphElement
	{
		public BaseGraphView	owner;
		public CommentBlock		commentBlock;

		public CommentBlockView()
		{
			AddStyleSheetPath("Styles/CommentBlock");
			
			Add(new Resizer());

			SetSize(Vector2.one * 50);
		}

		public void Initialize(BaseGraphView graphView, CommentBlock block)
		{
			commentBlock = block;
			owner = graphView;
		}
		
		public override void SetPosition(Rect newPosition)
		{
			base.SetPosition(newPosition);

			commentBlock.position = newPosition;
		}
	}
}