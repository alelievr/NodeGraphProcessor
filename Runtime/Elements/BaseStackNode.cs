using UnityEngine;
using System.Collections.Generic;

namespace GraphProcessor
{
    /// <summary>
    /// Data container for the StackNode views
    /// </summary>
    [System.Serializable]
    public class BaseStackNode
    {
        public Vector2 position;
        public string title = "New Stack";
        
        /// <summary>
        /// Is the stack accept drag and dropped nodes
        /// </summary>
        public bool acceptDrop;

        /// <summary>
        /// Is the stack accepting node created by pressing space over the stack node
        /// </summary>
        public bool acceptNewNode;

        /// <summary>
        /// List of node GUID that are in the stack
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public List< string >   nodeGUIDs = new List< string >();

        public BaseStackNode(Vector2 position, string title = "Stack", bool acceptDrop = true, bool acceptNewNode = true)
        {
            this.position = position;
            this.title = title;
            this.acceptDrop = acceptDrop;
            this.acceptNewNode = acceptNewNode;
        }
    }
}