using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace GraphProcessor
{
    public class MultiPorts
    {
        // The first MultiPortView class set this field to tell the other that she's controlling this MultiPort
        [System.NonSerialized]
        public object       viewController = null;

        public List< int >  portIds = new List< int >();

        public int          portCount => portIds.Count;

        public MultiPorts()
        {
            if (portIds.Count == 0)
                AddUniqueId(GetUniqueId());
        }

        public void RemoveUniqueId(int id)
        {
            portIds.Remove(id);
        }

        public void AddUniqueId(int id)
        {
            portIds.Add(id);
        }

        public int GetUniqueId()
        {
            int id = 0;
            
            while (portIds.Contains(id))
                id++;
            return id;
        }
    }
}