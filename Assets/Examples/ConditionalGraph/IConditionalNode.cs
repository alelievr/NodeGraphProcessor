using System.Collections.Generic;
using System.Reflection;

namespace NodeGraphProcessor.Examples
{
	interface IConditionalNode
	{
		IEnumerable< ConditionalNode >	GetExecutedNodes();

		FieldInfo[] GetNodeFields(); // Provide a custom order for fields (so conditional links are always at the top of the node)
	}
}