using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

namespace GraphProcessor
{
    public class CommentBlockView : Group
	{
		public BaseGraphView	owner;
		public CommentBlock		commentBlock;

        Label                   titleLabel;
        ColorField              colorField;

        readonly string         commentBlockStyle = "GraphProcessorStyles/CommentBlockView";

        public CommentBlockView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(commentBlockStyle));
		}

		public void Initialize(BaseGraphView graphView, CommentBlock block)
		{
			commentBlock = block;
			owner = graphView;

            title = block.title;
            SetPosition(block.position);

            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>(TitleChangedCallback);
            titleLabel = headerContainer.Q<Label>();

            colorField = new ColorField{ value = commentBlock.color, name = "headerColorPicker" };
            colorField.RegisterValueChangedCallback(e =>
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
                if (!owner.graph.nodesPerGUID.ContainsKey(nodeGUID))
                {
                    Debug.LogWarning("Node GUID not found: " + nodeGUID);

                    continue ;
                }
                var node = owner.graph.nodesPerGUID[nodeGUID];
                var nodeView = owner.nodeViewsPerNode[node];

                AddElement(nodeView);
            }
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                var node = element as BaseNodeView;

                if (node == null)
                    throw new System.ArgumentException("Adding another thing than node is not currently supported");

                if (!commentBlock.innerNodeGUIDs.Contains(node.nodeTarget.GUID))
                    commentBlock.innerNodeGUIDs.Add(node.nodeTarget.GUID);
            }
            base.OnElementsAdded(elements);
        }

        public void UpdateCommentBlockColor(Color newColor)
        {
            commentBlock.color = newColor;
            style.backgroundColor = newColor;
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