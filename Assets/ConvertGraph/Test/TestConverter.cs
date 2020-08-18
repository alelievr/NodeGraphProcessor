using UnityEngine;
using Cr7Sund.ConvertGraph;
using GraphProcessor;

[ConvertFunc("ConvertCommon/Convet2Color")]
public static class Convert2Color
{
    public static void Convert(int a, int b, float c, float d, out Color color)
    {
        color = new Vector4(a + 2, b, c, d);
    }
}

[ConvertFunc("ConvertCommon/ConvertV4ToFloats")]
public static class ConvertV4ToFloats
{
    public static void Convert(Vector4 v4, out float a, out float b, out float c, out float d)
    {
        a = v4.x;
        b = v4.y;
        c = v4.z;
        d = v4.w;
    }
}

[ConvertFunc("ConvertCommon/Convet2IntAndFloat")]
public static class Convet2IntAndFloat
{
    public static void Convert(float a, float b, float c, float d, out int oa, out float ob)
    {
        oa = (int)a;
        ob = b + c;
    }

}

[ConvertFunc("ConvertCommon/ConvetStr2Float")]
public static class ConvetStr2Float
{
    public static void Convert(string str, out int oa, out float ob)
    {
        oa = 2;
        ob = 4f;
    }

}

[ConvertFunc("ConvertCommon/ConvetBool2Str")]
public static class ConvetBool2Str
{
    public static void Convert(bool tf, out string str)
    {
        str = "Hello World";
    }
}


[ConvertFunc("ConvertBack/ConvetBackToStr")]
public static class ConvetBackToStr
{
    public static void Convert(float a, float d, out string str)
    {
        str = "Hello World";
    }
}

[ConvertFunc("ConvertBack/ConvertBackToBool")]
public static class ConvertBackToBool
{
    public static void Convert(float a, float d, out bool tf)
    {
        tf = true;
    }
}
