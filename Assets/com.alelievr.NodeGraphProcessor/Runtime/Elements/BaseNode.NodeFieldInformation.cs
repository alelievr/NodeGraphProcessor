using System.Reflection;

namespace GraphProcessor
{
    public abstract partial class BaseNode
    {
        internal class NodeFieldInformation
        {
            public string name;
            public string fieldName;
            public MemberInfo info;
            public bool input;
            public bool isMultiple;
            public string tooltip;
            public bool showAsDrawer;
            public CustomPortBehaviorDelegate behavior;
            public bool vertical;

            public NodeFieldInformation(MemberInfo info, string name, bool input, bool isMultiple, string tooltip, bool showAsDrawer, bool vertical, CustomPortBehaviorDelegate behavior)
            {
                this.input = input;
                this.isMultiple = isMultiple;
                this.info = info;
                this.name = name;
                this.fieldName = info.Name;
                this.behavior = behavior;
                this.tooltip = tooltip;
                this.showAsDrawer = showAsDrawer;
                this.vertical = vertical;
            }
        }
    }
}

