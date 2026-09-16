using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Triggers/REW Trigger Interact"), DisallowMultipleComponent]
    public sealed class REInteractTrigger : RETrigger
    {
        public string interactionText = "Use";
    }
}
