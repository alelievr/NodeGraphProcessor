using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

[System.Serializable]
public class BaseIsConditional
{
    [SerializeField, Input("True Checks", false, true)] protected BoolVariable[] trueChecks;
    public BoolVariable[] TrueChecks { get => trueChecks; }

    [SerializeField, Input("False Checks", false, true)] protected BoolVariable[] falseChecks;
    public BoolVariable[] FalseChecks { get => falseChecks; }

    public bool IsAvailable()
    {
        foreach (BoolVariable check in trueChecks)
        {
            if (!check.RuntimeValue) return false;
        }

        foreach (BoolVariable check in falseChecks)
        {
            if (check.RuntimeValue) return false;
        }
        return true;
    }
}

public interface IIsConditional
{
    List<BoolVariable> TrueChecks { get; }
    List<BoolVariable> FalseChecks { get; }

    bool IsAvailable();
}