using UnityEngine;
using VRC.SDK3.Components;

namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Triggers/REW Pickup Use Trigger")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(VRCPickup))]
    public sealed class REPickupUseTrigger : RETrigger
    {
    }
}
