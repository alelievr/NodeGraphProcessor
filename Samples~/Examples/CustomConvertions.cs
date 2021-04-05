using UnityEngine;
using GraphProcessor;
using System;
using NodeGraphProcessor.Examples;
using System.Collections.Generic;

public class CustomConvertions : ITypeAdapter
{
    public static Vector4 ConvertFloatToVector4(float from) => new Vector4(from, from, from, from);
    public static float ConvertVector4ToFloat(Vector4 from) => from.x;

    public override IEnumerable<(Type, Type)> GetIncompatibleTypes()
    {
        yield return (typeof(ConditionalLink), typeof(object));
        yield return (typeof(RelayNode.PackedRelayData), typeof(object));
    }
}