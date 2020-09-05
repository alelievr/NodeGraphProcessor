using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/Serialized Field Test")]
public class SerializedFieldTestNode : BaseNode
{

	[Input(name = "Vector 4"), SerializeField]
	public Vector4 vector4;

	[Input(name = "Vector 3"), SerializeField]
	public Vector3 vector3;

	[Input(name = "Vector 2"), SerializeField]
	public Vector2 vector2;

	[Input(name = "Float"), SerializeField]
	public float floatInput;

	[Input(name = "Int"), SerializeField]
	public int intInput;

	[Input(name = "Empty")]
	public int intInput2;

	[Input(name = "String"), SerializeField]
	public string stringInput;

	[Input(name = "Color"), SerializeField]
	public Color color;

	[Input(name = "Game Object"), SerializeField]
	public GameObject gameObject;

	[Input(name = "Animation Curve"), SerializeField]
	public AnimationCurve animationCurve;

	[Input(name = "Rigidbody"), SerializeField]
	public Rigidbody rigidbody;

	public override string name => "Serialized Field Test";

	protected override void Process() {}
}