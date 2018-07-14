using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine;

namespace GraphProcessor
{
	[System.Serializable]
	public class PinnedElement
	{
		public Vector2			position;
		public bool				opened;
		public SerializableType	editorType;
	}
}