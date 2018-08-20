using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor;
using UnityEngine;

namespace GraphProcessor
{
	public abstract class PinnedElementView : VisualElement
	{
		protected VisualElement header;
		protected PinnedElement	pinnedElement;

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

		public override void OnPersistentDataReady()
		{
			transform.position = pinnedElement.position;
		}

		public void InitializeGraphView(PinnedElement pinnedElement, BaseGraphView graphView)
		{
			this.pinnedElement = pinnedElement;
			transform.position = pinnedElement.position;
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