using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(MessageNode2))]
public class MessageNode2View : BaseNodeView
{
	public override void Enable()
	{
		var node = nodeTarget as MessageNode2;

		var icon = EditorGUIUtility.IconContent("UnityLogo").image;
		AddMessageView("Custom message !", icon, Color.green);
	}
}