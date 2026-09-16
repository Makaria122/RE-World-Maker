using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace REWorldMaker
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class REInteractTriggerRuntime : UdonSharpBehaviour
    {
        [Header("表示")]
        [SerializeField] private string interactionText = "使う";

        // Editor tooling fills this from RE Action components on the same GameObject.
        [SerializeField, HideInInspector] private UdonSharpBehaviour[] actions;

        private void Start()
        {
            InteractionText = interactionText;
        }

        public override void Interact()
        {
            REActionUtility.Execute(actions);
        }
    }
}
