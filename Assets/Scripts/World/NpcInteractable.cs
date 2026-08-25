using NinetyMinutes.Dialogue;
using UnityEngine;

namespace NinetyMinutes.World
{
    public sealed class NpcInteractable : Interactable
    {
        public string NpcId;
        public string RequireFlagMissing;
        public string DoneLine;
        public System.Func<DialogueGraph> GraphFactory;

        public override void Interact(PlayerController player)
        {
            if (DialogueRunner.Instance == null) return;
            WorldNpcBinder.Bind(this);
            if (GraphFactory == null) return;
            if (!string.IsNullOrEmpty(RequireFlagMissing) && SliceDialogues.Flags.Contains(RequireFlagMissing))
            {
                if (!string.IsNullOrEmpty(DoneLine))
                {
                    var g = new DialogueGraph { Id = "dlg_done_" + NpcId, StartNodeId = "n1" };
                    g.Nodes["n1"] = new DialogueNode
                    {
                        Id = "n1",
                        Speaker = "",
                        Line = DoneLine,
                        EndsDialogue = true
                    };
                    DialogueRunner.Instance.StartDialogue(g);
                }

                return;
            }

            DialogueRunner.Instance.StartDialogue(GraphFactory());
        }
    }
}
