using UnityEngine;
namespace REWorldMaker
{
    [AddComponentMenu("RE World Maker/Triggers/REW Player Trigger"), DisallowMultipleComponent, RequireComponent(typeof(BoxCollider))]
    public sealed class REPlayerTrigger : RETrigger
    {
        public bool executeOnEnter = true;
        public bool executeOnExit;
        public bool localPlayerOnly = true;
        public bool executeOnlyOnce;
        private void Reset() { BoxCollider box = GetComponent<BoxCollider>(); if (box != null) { box.isTrigger = true; box.size = new Vector3(2f, 2f, 2f); } }
    }
}
