using System.Reflection;

namespace GraphProcessor
{
    public abstract partial class BaseNode
    {
        /// <summary>
        /// Creates a node of type T at a certain position
        /// </summary>
        /// <param name="position">position in the graph in pixels</param>
        /// <typeparam name="T">type of the node</typeparam>
        /// <returns>the node instance</returns>
        public static T CreateFromType<T>(Vector2 position) where T : BaseNode
        {
            return CreateFromType(typeof(T), position) as T;
        }

        /// <summary>
        /// Creates a node of type nodeType at a certain position
        /// </summary>
        /// <param name="position">position in the graph in pixels</param>
        /// <typeparam name="nodeType">type of the node</typeparam>
        /// <returns>the node instance</returns>
        public static BaseNode CreateFromType(Type nodeType, Vector2 position)
        {
            if (!nodeType.IsSubclassOf(typeof(BaseNode)))
                return null;

            var node = Activator.CreateInstance(nodeType) as BaseNode;

            node.position = new Rect(position, new Vector2(100, 100));

            node.View = new ViewDelegates(node);

            ExceptionToLog.Call(() => node.OnNodeCreated());

            return node;
        }
    }
}