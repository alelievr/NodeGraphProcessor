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

        [System.NonSerialized]
        int                 currentIndex = 0;
        [SerializeField]
        List< int >         portIds = new List< int >();

        public int          portCount => portIds.Count;

        public int GetId()
        {
            if (portIds.Count > currentIndex)
                return portIds[currentIndex++];
            else
            {
                int id = GetUniqueId();
                portIds.Add(id);
                return id;
            }
        }

        int GetUniqueId()
        {
            int id = 0;
            
            while (portIds.Contains(id))
                id++;
            return id;
        }
    }
}