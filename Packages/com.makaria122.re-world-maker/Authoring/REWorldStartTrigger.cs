using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Triggers/REW Trigger World Start"), DisallowMultipleComponent]
    public sealed class REWorldStartTrigger : RETrigger { [Min(0f)] public float delaySeconds; }
}
