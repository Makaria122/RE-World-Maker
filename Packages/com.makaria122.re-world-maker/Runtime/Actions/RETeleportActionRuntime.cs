using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace REWorldMaker
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class RETeleportActionRuntime : UdonSharpBehaviour
    {
        [SerializeField] private Transform destination;

        public void _Execute()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer) || destination == null) return;
            localPlayer.TeleportTo(destination.position, destination.rotation);
        }
    }
}
