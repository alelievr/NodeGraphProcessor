using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
    public class CommentBlockView : Group
	{
		public BaseGraphView	owner;
		public CommentBlock		commentBlock;

        public CommentBlockView()
        {
            AddStyleSheetPath("GraphProcessorStyles/CommentBlockView");

            this.AddManipulator(new BorderResizer());
		}

		public void Initialize(BaseGraphView graphView, CommentBlock block)
		{
			commentBlock = block;
			owner = graphView;

            title = block.title;
			SetSize(block.size);
            SetPosition(block.position);

            headerContainer.Q< TextField >().RegisterCallback< ChangeEvent< string > >(TitleChangedCallback);
		}

        void TitleChangedCallback(ChangeEvent< string > e)
        {
            commentBlock.title = e.newValue;
        }
		
		public override void SetPosition(Rect newPos)
		{
			base.SetPosition(newPos);

			commentBlock.position = newPos;
		}
	}
}