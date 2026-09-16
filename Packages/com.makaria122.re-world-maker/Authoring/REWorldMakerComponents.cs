using UnityEngine;
using VRC.SDKBase;

namespace REWorldMaker
{
    public abstract class REAuthoringComponent : MonoBehaviour, IEditorOnly { }
    public abstract class RETrigger : REAuthoringComponent { }
    public abstract class REAction : REAuthoringComponent { }
    public enum REToggleOperation { Toggle, Enable, Disable }
    public enum REAnimatorParameterOperation { SetTrigger, SetBool, ToggleBool, SetInteger, SetFloat }
    public enum REAudioOperation { Play, Stop, Toggle }
}
