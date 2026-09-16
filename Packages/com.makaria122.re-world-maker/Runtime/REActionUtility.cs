using UdonSharp;

namespace REWorldMaker
{
    public static class REActionUtility
    {
        public const string ExecuteEvent = "_Execute";

        public static void Execute(UdonSharpBehaviour[] actions)
        {
            if (actions == null) return;

            for (int i = 0; i < actions.Length; i++)
            {
                UdonSharpBehaviour action = actions[i];
                if (action != null) action.SendCustomEvent(ExecuteEvent);
            }
        }
    }
}
