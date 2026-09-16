using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Actions/REW Synced Toggle Object Action"), DisallowMultipleComponent]
    public sealed class RESyncedToggleObjectAction : REAction { public GameObject[] targetObjects; public bool defaultEnabled = true; }
}
