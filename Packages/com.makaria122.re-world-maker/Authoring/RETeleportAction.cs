using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Actions/REW Action Teleport"), DisallowMultipleComponent]
    public sealed class RETeleportAction : REAction { public Transform destination; }
}
