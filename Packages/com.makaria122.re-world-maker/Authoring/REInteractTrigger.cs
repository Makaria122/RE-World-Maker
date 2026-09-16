using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Triggers/REW Interact Trigger"), DisallowMultipleComponent]
    public sealed class REInteractTrigger : RETrigger
    {
        public string interactionText = "Use";
    }
}
