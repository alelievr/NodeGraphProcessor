using System;
using System.Globalization;
using GraphProcessor;
using UnityEngine;

[Serializable, NodeMenuItem("Convert/Float to String"), ConverterNode(typeof(float), typeof(string))]
public class FloatToStringsNode : BaseNode, IConversionNode
{
    [Input("In")]
    public float input;

    public int decimalPlaces = 2;

    [Output("Out")]
    public string output;

    public override string name => "To String";

    public string GetConversionInput()
    {
        return nameof(input);
    }

    public string GetConversionOutput()
    {
        return nameof(output);
    }

    protected override void Process()
    {
        output = input.ToString("F" + decimalPlaces, CultureInfo.InvariantCulture);
    }
}
