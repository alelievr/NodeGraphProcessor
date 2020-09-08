using UnityEngine;
using Cr7Sund.ConvertGraph;
using System;
using GraphProcessor;

[ConvertClass("Custom/Converters")]  // Catalog, e.g. Menu Level 1: Custom, Menu Levl 2: Covnerters
public static class TestConverter
{
    [ConvertFunc("Mamba Out")]      // Converter description
    public static void ConvertWhatYouWant(int a, out int b, float c = 3)
    {
        b = (int)(a + c);
    }

    [ConvertFunc("Return the different type of string length")]
    public static void StrLegnth(string str, out float length, out int count)
    {
        length = Convert.ToInt32(str.Length);
        count = str.Length;
    }


    [ConvertFunc("Create a str with int")]
    public static void CreateStr(int a, out string str)
    {
        str = a.ToString();
    }

    [ConvertFunc("")]
    public static void Str2Bool(string str, out bool tf)
    {
        tf = str.Length == 0;
    }

    [ConvertFunc("")]
    public static void Bool2Str(bool tf, out string str)
    {
        str = tf ? "true" : "false";
    }
}