using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Actions/REW Action Toggle Object"), DisallowMultipleComponent]
    public sealed class REToggleObjectAction : REAction { public GameObject[] targetObjects; public REToggleOperation operation; }
}
