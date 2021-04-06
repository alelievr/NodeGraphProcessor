using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/Game Object")]
public class GameObjectNode : BaseNode, ICreateNodeFrom<GameObject>
{
	[Output(name = "Out"), SerializeField]
	public GameObject			output;

	public override string		name => "Game Object";

	public bool InitializeNodeFromObject(GameObject value)
	{
		output = value;
		return true;
	}
}
