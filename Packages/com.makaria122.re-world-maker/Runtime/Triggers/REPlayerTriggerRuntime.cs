using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace REWorldMaker
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class REPlayerTriggerRuntime : UdonSharpBehaviour
    {
        [Header("いつ実行するか")]
        [SerializeField] private bool executeOnEnter = true;
        [SerializeField] private bool executeOnExit;
        [SerializeField] private bool localPlayerOnly = true;
        [SerializeField] private bool executeOnlyOnce;

        // Editor tooling fills this from RE Action components on the same GameObject.
        [SerializeField, HideInInspector] private UdonSharpBehaviour[] actions;

        private bool _hasExecuted;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!executeOnEnter || !CanExecute(player)) return;
            ExecuteActions();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!executeOnExit || !CanExecute(player)) return;
            ExecuteActions();
        }

        private bool CanExecute(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return false;
            if (localPlayerOnly && !player.isLocal) return false;
            return !executeOnlyOnce || !_hasExecuted;
        }

        private void ExecuteActions()
        {
            _hasExecuted = true;
            REActionUtility.Execute(actions);
        }
    }
}
