using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace REWorldMaker
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class REToggleObjectActionRuntime : UdonSharpBehaviour
    {
        public const int Toggle = 0;
        public const int Enable = 1;
        public const int Disable = 2;

        [Header("対象")]
        [SerializeField] private GameObject[] targetObjects;

        [Tooltip("0: 切り替え / 1: 表示 / 2: 非表示")]
        [SerializeField] private int operation;

        public void _Execute()
        {
            if (targetObjects == null) return;

            for (int i = 0; i < targetObjects.Length; i++)
            {
                GameObject targetObject = targetObjects[i];
                if (targetObject == null) continue;

                if (operation == Enable) targetObject.SetActive(true);
                else if (operation == Disable) targetObject.SetActive(false);
                else targetObject.SetActive(!targetObject.activeSelf);
            }
        }
    }
}
