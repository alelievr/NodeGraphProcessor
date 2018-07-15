using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine;
using System;

namespace GraphProcessor
{
	[System.Serializable]
	public class PinnedElement
	{
		public Vector2			position = Vector2.zero;
		public bool				opened = true;
		public SerializableType	editorType;

		public PinnedElement(Type editorType)
		{
			this.editorType = new SerializableType(editorType);
		}
	}
}