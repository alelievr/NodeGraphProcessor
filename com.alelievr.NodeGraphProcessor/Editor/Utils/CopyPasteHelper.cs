using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	[System.Serializable]
	public class CopyPasteHelper
	{
		public List< JsonElement >	copiedNodes = new List< JsonElement >();

		public List< JsonElement >	copiedCommentBlocks = new List< JsonElement >();
	}
}