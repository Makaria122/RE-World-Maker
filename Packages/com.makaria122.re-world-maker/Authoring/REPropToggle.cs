using UnityEngine;
namespace REWorldMaker
{
    public enum REInitialState { UseCurrentState, StartOn, StartOff }

    [AddComponentMenu("RE World Maker/Actions/REW Prop Toggle"), DisallowMultipleComponent]
    public sealed class REPropToggle : REAction
    {
        public GameObject[] targetObjects;
        public REToggleOperation operation;
        public REInitialState initialState;
    }
}
