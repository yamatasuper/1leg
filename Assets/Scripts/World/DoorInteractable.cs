using NinetyMinutes.Dialogue;
using NinetyMinutes.Narrative;
using UnityEngine;

namespace NinetyMinutes.World
{
    public sealed class DoorInteractable : Interactable
    {
        public string TargetLocationId;
        public Vector2 TargetSpawn;
        public string RequireFlag;
        public string LockedLine;

        // Guards against the arrival spawn re-triggering the door the player just walked through.
        static float _reArmTime;

        void OnTriggerEnter(Collider other)
        {
            if (Time.time < _reArmTime) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || player.InputLocked) return;
            if (DialogueRunner.Instance != null && DialogueRunner.Instance.IsOpen) return;
            Interact(player);
        }

        public override void Interact(PlayerController player)
        {
            _reArmTime = Time.time + 1.5f;

            if (TargetLocationId == "loc_street"
                && SliceDirector.Instance != null
                && SliceDirector.Instance.Phase == SlicePhase.Training
                && !SliceDialogues.Flags.Contains("training_done"))
            {
                SliceDialogues.Flags.Add("training_skipped");
                SliceDialogues.Flags.Add("training_done");
                SliceDirector.Instance.EnterStreetLife();
                return;
            }

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
