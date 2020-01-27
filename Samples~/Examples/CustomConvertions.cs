using UnityEngine;
using GraphProcessor;

public class CustomConvertions : ITypeAdapter
{
    public static Vector4 ConvertFloatToVector4(float from) => new Vector4(from, from, from, from);
    public static float ConvertVector4ToFloat(Vector4 from) => from.x;
}