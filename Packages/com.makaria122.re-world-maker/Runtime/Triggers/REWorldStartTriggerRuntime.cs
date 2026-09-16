using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace REWorldMaker
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class REWorldStartTriggerRuntime : UdonSharpBehaviour
    {
        [Min(0f)]
        [SerializeField] private float delaySeconds;
        // Editor tooling fills this from RE Action components on the same GameObject.
        [SerializeField, HideInInspector] private UdonSharpBehaviour[] actions;

        private void Start()
        {
            if (delaySeconds <= 0f) _ExecuteActions();
            else SendCustomEventDelayedSeconds(nameof(_ExecuteActions), delaySeconds);
        }

        public void _ExecuteActions()
        {
            REActionUtility.Execute(actions);
        }
    }
}
