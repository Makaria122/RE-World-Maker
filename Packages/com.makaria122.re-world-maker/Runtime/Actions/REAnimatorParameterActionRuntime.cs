using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace REWorldMaker
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class REAnimatorParameterActionRuntime : UdonSharpBehaviour
    {
        public const int SetTrigger = 0;
        public const int SetBool = 1;
        public const int ToggleBool = 2;
        public const int SetInteger = 3;
        public const int SetFloat = 4;

        [SerializeField] private Animator animator;
        [SerializeField] private string parameterName;
        [Tooltip("0: Trigger / 1: Bool設定 / 2: Bool反転 / 3: Int / 4: Float")]
        [SerializeField] private int parameterType;
        [SerializeField] private bool boolValue = true;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;

        public void _Execute()
        {
            if (animator == null || string.IsNullOrEmpty(parameterName)) return;

            if (parameterType == SetBool) animator.SetBool(parameterName, boolValue);
            else if (parameterType == ToggleBool)
                animator.SetBool(parameterName, !animator.GetBool(parameterName));
            else if (parameterType == SetInteger) animator.SetInteger(parameterName, intValue);
            else if (parameterType == SetFloat) animator.SetFloat(parameterName, floatValue);
            else animator.SetTrigger(parameterName);
        }
    }
}
