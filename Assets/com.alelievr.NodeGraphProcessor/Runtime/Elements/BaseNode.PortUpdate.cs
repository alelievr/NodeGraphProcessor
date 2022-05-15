using System.Collections.Generic;

namespace GraphProcessor
{
    public abstract partial class BaseNode
    {
        private struct PortUpdate
        {
            public List<string> fieldNames;
            public BaseNode node;

            public void Deconstruct(out List<string> fieldNames, out BaseNode node)
            {
                fieldNames = this.fieldNames;
                node = this.node;
            }
        }
    }
}

