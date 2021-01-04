using UnityEngine;
using System;
using System.Collections.Generic;

namespace GraphProcessor
{
    [Serializable]
    public class ExposedParameterWorkaround : ScriptableObject
    {
        [SerializeReference]
        public List<ExposedParameter>   parameters = new List<ExposedParameter>();
        public BaseGraph                graph;
    }
}