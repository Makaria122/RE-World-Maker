using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace REWorldMaker
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class REAudioActionRuntime : UdonSharpBehaviour
    {
        public const int Play = 0;
        public const int Stop = 1;
        public const int Toggle = 2;

        [SerializeField] private AudioSource audioSource;
        [Tooltip("0: 再生 / 1: 停止 / 2: 再生・停止の切り替え")]
        [SerializeField] private int operation;

        private void Start()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        public void _Execute()
        {
            if (audioSource == null) return;
            if (operation == Stop) audioSource.Stop();
            else if (operation == Toggle)
            {
                if (audioSource.isPlaying) audioSource.Stop();
                else audioSource.Play();
            }
            else audioSource.Play();
        }
    }
}
