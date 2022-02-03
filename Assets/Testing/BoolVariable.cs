using UnityEngine;

[CreateAssetMenu(fileName = "BoolVariable", menuName = "Teletext/Variables/Bool")]
public class BoolVariable : SharedVariable<bool> 
{
    public override bool RuntimeValue { get => GetEvaluation(); set => base.RuntimeValue = value; }

    protected virtual bool GetEvaluation()
    {
        return base.RuntimeValue;
    }
}
