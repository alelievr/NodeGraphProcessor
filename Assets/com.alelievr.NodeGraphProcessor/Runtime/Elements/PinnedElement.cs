using System;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// Element that overlays the graph like the blackboard
	/// </summary>
	[Serializable]
    public class PinnedElement
    {
        #region Fields
        public Rect position = new(Vector2.zero, new Vector2(150, 200));
        public bool opened = true;
        public SerializableType editorType;
        #endregion

        #region Properties
        public virtual Vector2 DefaultSize  => new(150, 200);
        public virtual Vector2 DefaultPosition => Vector2.zero;
        #endregion

        #region Constructors
        public PinnedElement(Type editorType)
        {
            this.editorType = new SerializableType(editorType);
        }
        #endregion
    }
}