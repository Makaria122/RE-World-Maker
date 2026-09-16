using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Actions/REW Action Audio"), DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
    public sealed class REAudioAction : REAction
    { public AudioSource audioSource; public REAudioOperation operation; private void Reset() { audioSource = GetComponent<AudioSource>(); } }
}
