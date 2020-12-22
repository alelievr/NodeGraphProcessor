using System;
using UnityEngine;

namespace GraphProcessor
{
    /// <summary>
    /// Serializable Sticky node class
    /// </summary>
    [Serializable]
    public class StickyNote
    {
        public Rect position;
        public string title = "Hello World!";
        public string content = "Description";

        public StickyNote(string title, Vector2 position)
        {
            this.title = title;
            this.position = new Rect(position.x, position.y, 200, 300);
        }
    }
}