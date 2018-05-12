using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

public class EdgeView : Edge
{
	public bool			isConnected = false;

	public EdgeView()
	{
		AddStyleSheetPath("Styles/EdgeView");
	}
}