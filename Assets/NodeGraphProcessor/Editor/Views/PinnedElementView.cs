using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor;
using UnityEngine;

namespace GraphProcessor
{
	public abstract class PinnedElementView : GraphElement
	{
		protected VisualElement header;
        ResizeBorderFrame previewResizeBorderFrame;

		private Label titleLabel;

		public string title
		{
			get => titleLabel.text;
			set => titleLabel.text = value;
		}

		public PinnedElementView()
		{
            AddStyleSheetPath("GraphProcessorStyles/PinnedElementView");

			name = "PinnedElementView";

            clippingOptions = ClippingOptions.ClipAndCacheContents;
			header = new VisualElement { name = "header" };
			{
				titleLabel = new Label { name = "title", text = "Title" };
				header.Add(titleLabel);
			}
			Add(header);
		}

		public override void SetPosition(Rect newPos)
		{
			base.SetPosition(newPos);

			//TODO: save current position into graph
		}

		public void InitializeGraphView(BaseGraphView graphView)
		{
			Initialize(graphView);
		}

		protected abstract void Initialize(BaseGraphView graphView);

		~PinnedElementView()
		{
			Destroy();
		}

		protected virtual void Destroy() {}
	}
}