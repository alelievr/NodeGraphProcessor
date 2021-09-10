using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEditor;

[System.Serializable, NodeMenuItem("Custom/HeavyResourceNode")]
public class HeavyResourceNode : BaseNode
{
	public RenderTexture renderTexture;

    [MenuItem("MyMenu/Check Heavy Resources")]
    static void DoSomething()
    {
		var textures = Resources.FindObjectsOfTypeAll(typeof(RenderTexture));

		foreach (var texture in textures)
		{
			if (texture.name == "Heavy Resource")
				Debug.Log("Alive Texture: " + texture);
		}
    }

	[Input(name = "In")]
    public float                input;

	[Output(name = "Out")]
	public float				output;

	public override string		name => "HeavyResourceNode";

	public HeavyResourceNode()
	{
		Debug.Log("Create new node");
	}

	~HeavyResourceNode()
	{
		Debug.Log("Destroy node: " + renderTexture);
	}

	protected override void Enable()
	{
		renderTexture = new RenderTexture(4096, 4096, 0, RenderTextureFormat.ARGBFloat);
		renderTexture.name = "Heavy Resource";
		Debug.Log("Alloc!");
	}

	protected override void Disable()
	{
		Debug.Log("Release: " + renderTexture);
		if (renderTexture != null)
		{
			renderTexture.Release();
			Object.DestroyImmediate(renderTexture);
		}
	}

	protected override void Process()
	{
	    output = input * 42;
	}
}
