using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace GraphProcessor
{
	/// <summary>
	/// Element that overlays the graph like the blackboard
	/// </summary>
	[System.Serializable]
	public class PinnedElement
	{
		public static readonly Vector2	defaultSize = new Vector2(150, 200);

		public Rect				position = new Rect(Vector2.zero, defaultSize);
		public bool				opened = true;
		public SerializableType	editorType;

		public PinnedElement(Type editorType)
		{
			this.editorType = new SerializableType(editorType);
		}
	}
}