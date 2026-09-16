using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace REWorldMaker
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RESyncedToggleObjectActionRuntime : UdonSharpBehaviour
    {
        [SerializeField] private GameObject[] targetObjects;
        [SerializeField] private bool defaultEnabled = true;
        [SerializeField] private int operation;

        [UdonSynced] private bool _isEnabled;
        [UdonSynced] private bool _initialized;

        private void Start()
        {
            if (!_initialized && Networking.IsOwner(gameObject))
            {
                _isEnabled = defaultEnabled;
                _initialized = true;
                ApplyState();
                RequestSerialization();
            }
        }

        public void _Execute()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(localPlayer, gameObject);

            if (!Networking.IsOwner(gameObject)) return;

            if (operation == 1) _isEnabled = true;
            else if (operation == 2) _isEnabled = false;
            else _isEnabled = !_isEnabled;
            _initialized = true;
            ApplyState();
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            ApplyState();
        }

        private void ApplyState()
        {
            if (!_initialized || targetObjects == null) return;
            for (int i = 0; i < targetObjects.Length; i++)
            {
                GameObject targetObject = targetObjects[i];
                if (targetObject != null) targetObject.SetActive(_isEnabled);
            }
        }
    }
}
