using NinetyMinutes.Dialogue;
using NinetyMinutes.Match;
using UnityEngine;

namespace NinetyMinutes.World
{
    public sealed class PlayerController : MonoBehaviour
    {
        public float Speed = 5.2f;
        public WorldCameraRig CameraRig;

        Rigidbody _rb;
        Vector3 _move;
        public bool InputLocked { get; set; }
        public bool IsMoving => _move.sqrMagnitude > 0.01f;

        void Awake()
        {
            WorldSceneFactory.EnsurePlayerPhysics(gameObject);
            _rb = GetComponent<Rigidbody>();
            if (GetComponent<WalkFlipbook>() == null)
                gameObject.AddComponent<WalkFlipbook>();
        }

        void Update()
        {
            var matchPlaying = MatchPresentation.Instance != null && MatchPresentation.Instance.IsPlaying;
            if (InputLocked || matchPlaying || (DialogueRunner.Instance != null && DialogueRunner.Instance.IsOpen))
            {
                _move = Vector3.zero;
                return;
            }

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector3 fwd = Vector3.forward;
            Vector3 right = Vector3.right;
            if (CameraRig != null)
            {
                fwd = CameraRig.PlanarForward;
                right = CameraRig.PlanarRight;
            }

            _move = right * input.x + fwd * input.y;
            if (_move.sqrMagnitude > 1f) _move.Normalize();

            if (_move.sqrMagnitude > 0.01f)
            {
                var look = Quaternion.LookRotation(_move, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 12f * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                TryInteract();

            WorldController.Instance?.SetFocusPrompt(FindNearby()?.Prompt);
        }

        void FixedUpdate()
        {
            if (_rb == null) return;
            var v = _move * Speed;
            _rb.velocity = new Vector3(v.x, 0f, v.z);
        }

        /// <summary>
        /// Position relative to the active location origin — the same space <see cref="Place"/> expects.
        /// </summary>
        public Vector2 LocalGroundPos
        {
            get
            {
                var local = transform.position - Origin;
                return new Vector2(local.x, local.z);
            }
        }

        public void Place(Vector2 spawn)
        {
            transform.position = Origin + new Vector3(spawn.x, 0f, spawn.y);
            if (_rb != null) _rb.velocity = Vector3.zero;
        }

        static Vector3 Origin => WorldController.Instance != null
            ? WorldController.Instance.ActiveOrigin
            : Vector3.zero;

        void TryInteract()
        {
            FindNearby()?.Interact(this);
        }

        Interactable FindNearby()
        {
            var origin = transform.position + Vector3.up * 0.9f;
            var hits = Physics.OverlapSphere(origin, 2.4f, ~0, QueryTriggerInteraction.Collide);
            Interactable best = null;
            var bestDist = float.MaxValue;
            foreach (var h in hits)
            {
                var i = h.GetComponent<Interactable>() ?? h.GetComponentInParent<Interactable>();
                if (i == null || i.gameObject == gameObject) continue;
                var d = (h.transform.position - transform.position).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            return best;
        }
    }
}
