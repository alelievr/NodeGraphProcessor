using System;
using GraphProcessor;
using UnityEngine;
public static class CustomMenuEntry
{
    [CustomMenuItem("Custom/PresetFloatNode")]
    public static FloatNode DoCustomNodeCreation(Type type, Vector2 mouseLocation)
    {
        FloatNode node = BaseNode.CreateFromType<FloatNode>(mouseLocation);
        node.SetCustomName("Custom Creation");
        node.input = 1337;
        return node;
    }
}
