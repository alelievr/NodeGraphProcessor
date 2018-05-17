using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	[System.Serializable]
	public class CommentBlock
	{
		public string			title;
		public Color			color;
		public Rect				position;

		public List< string >	innerNodeGUIDs = new List< string >();

		public CommentBlock(string title)
		{
			this.title = title;
		}

	}
}