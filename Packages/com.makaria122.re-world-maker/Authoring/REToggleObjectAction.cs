using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Actions/REW Toggle Object Action"), DisallowMultipleComponent]
    public sealed class REToggleObjectAction : REAction { public GameObject[] targetObjects; public REToggleOperation operation; }
}
