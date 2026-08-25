using NinetyMinutes.Dialogue;
using UnityEngine;

namespace NinetyMinutes.World
{
    public sealed class DoorInteractable : Interactable
    {
        public string TargetLocationId;
        public Vector2 TargetSpawn;
        public string RequireFlag;
        public string LockedLine;

        public override void Interact(PlayerController player)
        {
            if (!string.IsNullOrEmpty(RequireFlag))
            {
                var ok = RequireFlag != "__never__" && SliceDialogues.Flags.Contains(RequireFlag);
                if (!ok)
                {
                    if (!string.IsNullOrEmpty(LockedLine) && DialogueRunner.Instance != null)
                    {
                        var g = new DialogueGraph { Id = "dlg_door_locked", StartNodeId = "n1" };
                        g.Nodes["n1"] = new DialogueNode
                        {
                            Id = "n1",
                            Speaker = "",
                            Line = LockedLine,
                            EndsDialogue = true
                        };
                        DialogueRunner.Instance.StartDialogue(g);
                    }

                    return;
                }
            }

            WorldController.Instance?.TravelTo(TargetLocationId, TargetSpawn);
        }
    }
}
