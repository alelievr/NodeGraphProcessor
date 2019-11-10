using System.Collections;
using System.Collections.Generic;

interface IConditionalNode
{
	IEnumerable< ConditionalNode >	GetExecutedNodes();
}