using UnityEngine;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;

namespace GraphProcessor
{
    public class CommentBlockView : Group
	{
		public BaseGraphView	owner;
		public CommentBlock		commentBlock;

        Label                   titleLabel;
        ColorField              colorField;

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

            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>(TitleChangedCallback);
            titleLabel = headerContainer.Q<Label>();

            colorField = new ColorField{ value = commentBlock.color, name = "headerColorPicker" };
            colorField.OnValueChanged(e =>
            {
                UpdateCommentBlockColor(e.newValue);
            });
            UpdateCommentBlockColor(commentBlock.color);

            headerContainer.Add(colorField);

            InitializeInnerNodes();
		}

        void InitializeInnerNodes()
        {
            foreach (var nodeGUID in commentBlock.innerNodeGUIDs)
            {
                var node = owner.graph.nodesPerGUID[nodeGUID];
                var nodeView = owner.nodeViewsPerNode[node];

                AddElement(nodeView);
            }
        }

        protected override void OnElementAdded(GraphElement element)
        {
            var node = element as BaseNodeView;

            if (node == null)
                throw new System.ArgumentException("Adding another thing than node is not currently supported");

            if (!commentBlock.innerNodeGUIDs.Contains(node.nodeTarget.GUID))
                commentBlock.innerNodeGUIDs.Add(node.nodeTarget.GUID);

            base.OnElementAdded(element);
        }

        protected override void OnElementRemoved(GraphElement element)
        {
            base.OnElementRemoved(element);
        }

        public void UpdateCommentBlockColor(Color newColor)
        {
            commentBlock.color = newColor;
            style.backgroundColor = newColor;
            titleLabel.style.textColor = new Color(1 - newColor.r, 1 - newColor.g, 1 - newColor.b, 1);
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