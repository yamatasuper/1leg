using NinetyMinutes.Dialogue;
using NinetyMinutes.Match;
using UnityEngine;

namespace NinetyMinutes.World
{
    public sealed class PlayerController : MonoBehaviour
    {
        public float Speed = 4.5f;
        Rigidbody2D _rb;
        Vector2 _input;
        public bool InputLocked { get; set; }

        void Awake()
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
        }

        void Update()
        {
            var matchPlaying = MatchPresentation.Instance != null && MatchPresentation.Instance.IsPlaying;
            if (InputLocked || matchPlaying || (DialogueRunner.Instance != null && DialogueRunner.Instance.IsOpen))
            {
                _input = Vector2.zero;
                return;
            }

            _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (_input.sqrMagnitude > 1f) _input.Normalize();

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                TryInteract();
        }

        void FixedUpdate()
        {
            _rb.velocity = _input * Speed;
        }

        void TryInteract()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, 0.85f);
            Interactable best = null;
            var bestDist = float.MaxValue;
            foreach (var h in hits)
            {
                var i = h.GetComponent<Interactable>();
                if (i == null) continue;
                var d = ((Vector2)h.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            best?.Interact(this);
        }
    }

    public abstract class Interactable : MonoBehaviour
    {
        public string Prompt = "E — взаимодействие";
        public abstract void Interact(PlayerController player);
    }

    public sealed class NpcInteractable : Interactable
    {
        public string NpcId;
        public System.Func<DialogueGraph> GraphFactory;
        public string RequireFlagMissing; // if set and flag present → refuse
        public string DoneLine;

        public override void Interact(PlayerController player)
        {
            if (GraphFactory == null || DialogueRunner.Instance == null) return;
            if (!string.IsNullOrEmpty(RequireFlagMissing) && SliceDialogues.Flags.Contains(RequireFlagMissing))
            {
                if (!string.IsNullOrEmpty(DoneLine) && DialogueRunner.Instance != null)
                {
                    // tiny one-liner via ephemeral graph
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
