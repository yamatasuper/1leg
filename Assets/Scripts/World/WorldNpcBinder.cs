using NinetyMinutes.Dialogue;

namespace NinetyMinutes.World
{
    public static class WorldNpcBinder
    {
        public static void Bind(NpcInteractable npc)
        {
            if (npc == null) return;
            switch (npc.NpcId)
            {
                case "npc_coach":
                    npc.GraphFactory = SliceDialogues.TrainingCoach;
                    break;
                case "skip_training":
                    npc.GraphFactory = SliceDialogues.TrainingSkip;
                    break;
                case "npc_glock":
                    npc.GraphFactory = SliceDialogues.Segment1Glock;
                    break;
                case "npc_sokol":
                    npc.GraphFactory = SliceDialogues.Segment2Sokol;
                    break;
                case "self_thought":
                    npc.GraphFactory = SliceDialogues.Segment3Self;
                    break;
            }
        }
    }
}
