using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace REWorldMaker
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RESyncedAudioActionRuntime : UdonSharpBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private int operation;

        [UdonSynced] private bool _isPlaying;
        [UdonSynced] private double _startServerTime;
        [UdonSynced] private bool _initialized;

        public void _Execute()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer) || audioSource == null) return;
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
            if (!Networking.IsOwner(gameObject)) return;

            if (operation == 1) StopGlobal();
            else if (operation == 2 && _isPlaying) StopGlobal();
            else PlayGlobal();

            RequestSerialization();
        }

        private void PlayGlobal()
        {
            _isPlaying = true;
            _initialized = true;
            _startServerTime = Networking.GetServerTimeInSeconds();
            ApplyState();
        }

        private void StopGlobal()
        {
            _isPlaying = false;
            _initialized = true;
            ApplyState();
        }

        public override void OnDeserialization()
        {
            ApplyState();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (Utilities.IsValid(player) && player.isLocal) RequestSerialization();
        }

        private void ApplyState()
        {
            if (!_initialized || audioSource == null) return;
            if (!_isPlaying)
            {
                audioSource.Stop();
                return;
            }

            AudioClip clip = audioSource.clip;
            if (clip == null || clip.length <= 0f) return;
            double elapsed = Networking.GetServerTimeInSeconds() - _startServerTime;
            if (elapsed < 0d) elapsed = 0d;
            if (!audioSource.loop && elapsed >= clip.length)
            {
                audioSource.Stop();
                return;
            }
            audioSource.time = (float)(elapsed % clip.length);
            audioSource.Play();
        }
    }
}
