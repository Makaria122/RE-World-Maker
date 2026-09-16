using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Actions/REW Teleport Action"), DisallowMultipleComponent]
    public sealed class RETeleportAction : REAction { public Transform destination; }
}
