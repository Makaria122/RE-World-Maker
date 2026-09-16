using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Actions/REW Action Animator Parameter"), DisallowMultipleComponent]
    public sealed class REAnimatorParameterAction : REAction
    { public Animator animator; public string parameterName; public REAnimatorParameterOperation operation; public bool boolValue = true; public int intValue; public float floatValue; }
}
