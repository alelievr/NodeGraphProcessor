using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/Drawer Field Test")]
public class DrawerFieldTestNode : BaseNode
{

    [Input(name = "Vector 4", showAsDrawer = true)]
    public Vector4 vector4;

    [Input(name = "Vector 3", showAsDrawer = true)]
    public Vector3 vector3;

    [Input(name = "Vector 2", showAsDrawer = true)]
    public Vector2 vector2;

    [Input(name = "Float", showAsDrawer = true)]
    public float floatInput;

    [Input(name = "Vector 3 Int", showAsDrawer = true)]
    public Vector3Int vector3Int;

    [Input(name = "Vector 2 Int", showAsDrawer = true)]
    public Vector2Int vector2Int;

    [Input(name = "Int", showAsDrawer = true)]
    public int intInput;

    [Input(name = "Empty")]
    public int intInput2;

    [Input(name = "String", showAsDrawer = true)]
    public string stringInput;

    [Input(name = "Color", showAsDrawer = true)]
    new public Color color;

    [Input(name = "Game Object", showAsDrawer = true)]
    public GameObject gameObject;

    [Input(name = "Animation Curve", showAsDrawer = true)]
    public AnimationCurve animationCurve;

    [Input(name = "Rigidbody", showAsDrawer = true)]
    public Rigidbody rigidbody;

    [Input("Layer Mask", showAsDrawer = true)]
    public LayerMask layerMask;

    public override string name => "Drawer Field Test";

    protected override void Process() { }
}