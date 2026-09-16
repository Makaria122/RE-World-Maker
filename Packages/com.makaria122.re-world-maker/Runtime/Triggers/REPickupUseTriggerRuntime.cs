using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace REWorldMaker
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class REPickupUseTriggerRuntime : UdonSharpBehaviour
    {
        // Editor tooling fills this from REW Action components on the same GameObject.
        [SerializeField, HideInInspector] private UdonSharpBehaviour[] actions;

        public override void OnPickupUseDown()
        {
            REActionUtility.Execute(actions);
        }
    }
}
