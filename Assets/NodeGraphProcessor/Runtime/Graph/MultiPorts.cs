using System;
using UnityEngine;

namespace GraphProcessor
{
    public class MultiPorts
    {
        public event Action OnPortCountChanged;
        public event Action OnPortAdded;
        public event Action OnPortRemoved;
    }
}