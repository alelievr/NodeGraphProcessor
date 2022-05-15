using UnityEngine;
using System;

namespace GraphProcessor
{
    public class ViewDelegates
    {
        private readonly BaseNode _node;
        private readonly Func<Rect> _getRect;
        private readonly Action<Rect> _setRect;

        public ViewDelegates(BaseNode node)
        {
            this._node = node;
        }

        public ViewDelegates(BaseNode node, Func<Rect> getRect, Action<Rect> setRect)
        {
            this._node = node;
            this._getRect = getRect;
            this._setRect = setRect;
        }

        public Func<Rect> GetRect => _getRect ?? (() => _node.initialPosition);
        public Action<Rect> SetRect => _setRect ?? ((rect) => _node.initialPosition = rect);

        public Vector2 GetPosition() => GetRect().position;
        public void SetPosition(Vector2 position) => SetRect(new Rect(position, GetSize()));

        public Vector2 GetSize() => GetRect().size;
        // public void SetSize(Vector2 size) => SetRect(new Rect(GetPosition(), size));
    }
}

