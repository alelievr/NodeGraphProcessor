using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace GraphProcessor
{
	public class EdgeView : Edge
	{
		public bool					isConnected = false;

		public SerializableEdge		serializedEdge { get { return userData as SerializableEdge; } }

		readonly string				edgeStyle = "GraphProcessorStyles/EdgeView";

		public EdgeView() : base()
		{
			styleSheets.Add(Resources.Load<StyleSheet>(edgeStyle));
		}

        public override void OnPortChanged(bool isInput) => UpdateEdgeSize();

		public void UpdateEdgeSize()
		{
			if (input == null && output == null)
				return;

			PortData inputPortData = (input as PortView)?.portData;
			PortData outputPortData = (output as PortView)?.portData;

			for (int i = 1; i < 20; i++)
				RemoveFromClassList($"edge_{i}");
			int maxPortSize = Mathf.Max(inputPortData?.sizeInPixel ?? 0, outputPortData?.sizeInPixel ?? 0);
			if (maxPortSize > 0)
				AddToClassList($"edge_{Mathf.Max(1, maxPortSize - 6)}");
		}

		protected override void OnCustomStyleResolved(ICustomStyle styles)
		{
			base.OnCustomStyleResolved(styles);

			UpdateEdgeControl();
		}
	}
}