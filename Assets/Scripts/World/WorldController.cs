using NinetyMinutes.Art;
using NinetyMinutes.Core;
using NinetyMinutes.Dialogue;
using NinetyMinutes.Narrative;
using NinetyMinutes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NinetyMinutes.World
{
    public sealed class WorldController : MonoBehaviour
    {
        public static WorldController Instance { get; private set; }

        GameObject _worldRoot;
        GameObject _locker;
        GameObject _street;
        PlayerController _player;
        Camera _worldCam;
        Text _hudHint;
        GameObject _hudRoot;

        GameObject _npcCoachLocker;
        GameObject _npcSkipTraining;
        GameObject _npcGlockStreet;
        GameObject _npcSokolStreet;
        GameObject _npcSelfThought;
        DoorInteractable _doorToStreet;
        DoorInteractable _doorToLocker;

        public string CurrentLocationId { get; private set; } = "loc_locker";
        public bool IsActive => _worldRoot != null && _worldRoot.activeSelf;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void StartOrResumeWorld(string locationId, Vector2? spawn = null)
        {
            EnsureWorldBuilt();
            _worldRoot.SetActive(true);
            if (_player != null) _player.gameObject.SetActive(true);
            EnableWorldCamera(true);
            TravelTo(string.IsNullOrEmpty(locationId) ? "loc_locker" : locationId, spawn);
            ShowHud(true);
            if (_player != null) _player.InputLocked = false;
            if (SliceDirector.Instance != null)
                RefreshSpine(SliceDirector.Instance.Phase);
        }

        public void StopWorld()
        {
            if (_player != null) _player.InputLocked = true;
            ShowHud(false);
            EnableWorldCamera(false);
            if (_worldRoot != null) _worldRoot.SetActive(false);
        }

        public void SuspendForMatch(bool suspend)
        {
            if (_player != null) _player.InputLocked = suspend;
            ShowHud(!suspend && IsActive);
            EnableWorldCamera(!suspend && _worldRoot != null && _worldRoot.activeSelf);
        }

        public void TravelTo(string locationId, Vector2? spawn = null)
        {
            EnsureWorldBuilt();
            CurrentLocationId = locationId;
            if (GameSession.Instance != null)
                GameSession.Instance.LocationId = locationId;

            var toLocker = locationId == "loc_locker";
            if (_locker != null) _locker.SetActive(toLocker);
            if (_street != null) _street.SetActive(!toLocker);

            // Player lives under world root (not location) so stays visible on both maps.
            if (_player != null)
            {
                _player.gameObject.SetActive(true);
                var pos = spawn ?? new Vector2(0f, -1.5f);
                _player.transform.position = new Vector3(pos.x, pos.y, -1f);
                var rb = _player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;
            }

            UpdateHud();
        }

        public void RefreshSpine(SlicePhase phase)
        {
            EnsureWorldBuilt();

            var street = phase == SlicePhase.StreetLife;
            SetActive(_npcCoachLocker, phase == SlicePhase.Training);
            SetActive(_npcSkipTraining, phase == SlicePhase.Training && !SliceDialogues.Flags.Contains("training_done"));
            SetActive(_npcGlockStreet, street && !SliceDialogues.Flags.Contains("street_glock_done"));
            SetActive(_npcSokolStreet, street && !SliceDialogues.Flags.Contains("street_sokol_done"));
            SetActive(_npcSelfThought, street && !SliceDialogues.Flags.Contains("street_self_done"));

            if (_doorToStreet != null)
            {
                if (phase == SlicePhase.Training)
                {
                    _doorToStreet.RequireFlag = "training_done";
                    _doorToStreet.LockedLine = "Сначала тренировка — или сознательный отказ от неё.";
                }
                else
                {
                    _doorToStreet.RequireFlag = null;
                    _doorToStreet.LockedLine = null;
                }
            }

            if (_doorToLocker != null)
            {
                // During street life stay focused on dialogues; locker optional after training.
                _doorToLocker.RequireFlag = null;
                _doorToLocker.LockedLine = null;
            }

            if (_npcCoachLocker != null)
            {
                var npc = _npcCoachLocker.GetComponent<NpcInteractable>();
                if (npc != null)
                {
                    npc.Prompt = "E — говорить с тренером";
                    npc.GraphFactory = SliceDialogues.TrainingCoach;
                    npc.RequireFlagMissing = "training_done";
                    npc.DoneLine = "Тренировка уже позади.";
                }
            }

            UpdateHud();
        }

        static void SetActive(GameObject go, bool on)
        {
            if (go != null) go.SetActive(on);
        }

        void EnsureWorldBuilt()
        {
            if (_worldRoot != null) return;

            _worldRoot = new GameObject("WorldRoot");
            DontDestroyOnLoad(_worldRoot);

            var camGo = new GameObject("WorldCamera");
            camGo.transform.SetParent(_worldRoot.transform, false);
            _worldCam = camGo.AddComponent<Camera>();
            _worldCam.orthographic = true;
            _worldCam.orthographicSize = 5.5f;
            _worldCam.clearFlags = CameraClearFlags.SolidColor;
            _worldCam.backgroundColor = new Color(0.05f, 0.06f, 0.07f);
            _worldCam.depth = 10;
            _worldCam.transform.position = new Vector3(0, 0, -10);
            camGo.AddComponent<AudioListener>();
            foreach (var al in FindObjectsOfType<AudioListener>())
            {
                if (al.gameObject != camGo) al.enabled = false;
            }

            BuildLocker();
            BuildStreet();
            BuildPlayer();
            BuildHud();

            _locker.SetActive(true);
            _street.SetActive(false);
        }

        void BuildLocker()
        {
            _locker = new GameObject("loc_locker");
            _locker.transform.SetParent(_worldRoot.transform, false);

            WorldSprites.SpriteGo("FloorArt", ArtCatalog.LocationLocker, new Vector2(16, 10), _locker.transform, 0);
            BuildBounds(_locker.transform, 16, 10);

            _npcCoachLocker = WorldSprites.SpriteGo("npc_coach", ArtCatalog.PortraitCoach, new Vector2(1.6f, 1.6f), _locker.transform, 5);
            _npcCoachLocker.transform.position = new Vector3(2.2f, 0.3f, -0.5f);
            var col = _npcCoachLocker.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1f);
            var interact = _npcCoachLocker.AddComponent<NpcInteractable>();
            interact.NpcId = "npc_coach";
            interact.Prompt = "E — говорить с тренером";
            interact.GraphFactory = SliceDialogues.TrainingCoach;

            _npcSkipTraining = WorldSprites.Quad("skip_training", new Vector2(1.2f, 0.5f), new Color(0.35f, 0.2f, 0.2f), _locker.transform, 5);
            _npcSkipTraining.transform.position = new Vector3(-3.5f, -2.2f, -0.5f);
            var scol = _npcSkipTraining.AddComponent<BoxCollider2D>();
            scol.isTrigger = true;
            var skip = _npcSkipTraining.AddComponent<NpcInteractable>();
            skip.NpcId = "skip_training";
            skip.Prompt = "E — пропустить тренировку";
            skip.GraphFactory = SliceDialogues.TrainingSkip;

            var door = WorldSprites.Quad("door_to_street", new Vector2(1.4f, 2.2f), new Color(0.25f, 0.4f, 0.55f), _locker.transform, 4);
            door.transform.position = new Vector3(-6.5f, 0f, -0.5f);
            var dcol = door.AddComponent<BoxCollider2D>();
            dcol.isTrigger = true;
            _doorToStreet = door.AddComponent<DoorInteractable>();
            _doorToStreet.Prompt = "E — на бровку";
            _doorToStreet.TargetLocationId = "loc_street";
            _doorToStreet.TargetSpawn = new Vector2(5.5f, 0f);
        }

        void BuildStreet()
        {
            _street = new GameObject("loc_street");
            _street.transform.SetParent(_worldRoot.transform, false);

            WorldSprites.SpriteGo("FloorArt", ArtCatalog.LocationStreet, new Vector2(16, 10), _street.transform, 0);
            BuildBounds(_street.transform, 16, 10);

            var door = WorldSprites.Quad("door_to_locker", new Vector2(1.4f, 2.2f), new Color(0.35f, 0.3f, 0.2f), _street.transform, 4);
            door.transform.position = new Vector3(6.5f, 0f, -0.5f);
            var dcol = door.AddComponent<BoxCollider2D>();
            dcol.isTrigger = true;
            _doorToLocker = door.AddComponent<DoorInteractable>();
            _doorToLocker.Prompt = "E — в раздевалку";
            _doorToLocker.TargetLocationId = "loc_locker";
            _doorToLocker.TargetSpawn = new Vector2(-5.5f, 0f);

            _npcGlockStreet = WorldSprites.SpriteGo("npc_glock", ArtCatalog.PortraitGlock, new Vector2(1.5f, 1.5f), _street.transform, 5);
            _npcGlockStreet.transform.position = new Vector3(-2.5f, 0.2f, -0.5f);
            var lcol = _npcGlockStreet.AddComponent<BoxCollider2D>();
            lcol.isTrigger = true;
            var interact = _npcGlockStreet.AddComponent<NpcInteractable>();
            interact.NpcId = "npc_glock";
            interact.Prompt = "E — говорить с Глоком";
            interact.GraphFactory = SliceDialogues.Segment1Glock;
            interact.RequireFlagMissing = "street_glock_done";
            interact.DoneLine = "С Глоком уже поговорили.";

            _npcSokolStreet = WorldSprites.SpriteGo("npc_sokol", ArtCatalog.PortraitSokol, new Vector2(1.6f, 1.6f), _street.transform, 5);
            _npcSokolStreet.transform.position = new Vector3(1.5f, 0.3f, -0.5f);
            var ccol = _npcSokolStreet.AddComponent<BoxCollider2D>();
            ccol.isTrigger = true;
            var sokol = _npcSokolStreet.AddComponent<NpcInteractable>();
            sokol.NpcId = "npc_sokol";
            sokol.Prompt = "E — говорить с Соколом";
            sokol.GraphFactory = SliceDialogues.Segment2Sokol;
            sokol.RequireFlagMissing = "street_sokol_done";
            sokol.DoneLine = "С Соколом уже поговорили.";

            _npcSelfThought = WorldSprites.SpriteGo("self_thought", ArtCatalog.PortraitBardin, new Vector2(1.4f, 1.4f), _street.transform, 5);
            _npcSelfThought.transform.position = new Vector3(-0.5f, 0.15f, -0.5f);
            var scol = _npcSelfThought.AddComponent<BoxCollider2D>();
            scol.isTrigger = true;
            var self = _npcSelfThought.AddComponent<NpcInteractable>();
            self.NpcId = "self_thought";
            self.Prompt = "E — остаться с собой";
            self.GraphFactory = SliceDialogues.Segment3Self;
            self.RequireFlagMissing = "street_self_done";
            self.DoneLine = "Этот разговор уже был.";
        }

        void BuildBounds(Transform parent, float w, float h)
        {
            float hw = w * 0.5f;
            float hh = h * 0.5f;
            MakeWall(parent, "WallN", new Vector2(0, hh + 0.25f), new Vector2(w + 1, 0.5f));
            MakeWall(parent, "WallS", new Vector2(0, -hh - 0.25f), new Vector2(w + 1, 0.5f));
            MakeWall(parent, "WallE", new Vector2(hw + 0.25f, 0), new Vector2(0.5f, h + 1));
            MakeWall(parent, "WallW", new Vector2(-hw - 0.25f, 0), new Vector2(0.5f, h + 1));
        }

        void MakeWall(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = WorldSprites.Quad(name, size, new Color(0.05f, 0.05f, 0.06f), parent, 1);
            go.transform.position = new Vector3(pos.x, pos.y, 0);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        void BuildPlayer()
        {
            var p = WorldSprites.SpriteGo("Player", ArtCatalog.SpritePlayer, new Vector2(1.1f, 1.1f), _worldRoot.transform, 20);
            if (p.GetComponent<SpriteRenderer>().sprite == null || ArtCatalog.SpritePlayer == null)
            {
                // fallback bright marker
                p.GetComponent<SpriteRenderer>().sprite = WorldSprites.Pixel;
                p.GetComponent<SpriteRenderer>().color = new Color(1f, 0.92f, 0.25f);
                p.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            }

            p.transform.position = new Vector3(0, -1.5f, -1f);
            _player = p.AddComponent<PlayerController>();
        }

        void BuildHud()
        {
            var canvas = UiFactory.CreateCanvas("WorldHudCanvas", 150);
            DontDestroyOnLoad(canvas.gameObject);
            _hudRoot = canvas.gameObject;
            var panel = UiFactory.Box(canvas.transform, "HintBox", new Vector2(0, 460), new Vector2(1100, 70),
                new Color(0.05f, 0.06f, 0.08f, 0.75f));
            _hudHint = UiFactory.Label(panel, "Hint", "", 22, TextAnchor.MiddleCenter, Color.white);
            ShowHud(false);
        }

        void UpdateHud()
        {
            if (_hudHint == null) return;
            var loc = CurrentLocationId == "loc_locker" ? "Раздевалка «Торпедо»" : "Бровка";
            if (SliceDirector.Instance != null && SliceDirector.Instance.Phase == SlicePhase.StreetLife)
            {
                var left = 0;
                foreach (var f in SliceDirector.StreetDialogueFlags)
                    if (!SliceDialogues.Flags.Contains(f)) left++;
                _hudHint.text = $"{loc} · поговори со всеми ({3 - left}/3) · потом матч · WASD / E";
            }
            else
            {
                var phase = SliceDirector.Instance != null ? SliceDirector.Instance.Phase.ToString() : "";
                _hudHint.text = $"{loc} · {phase} · WASD · E · Tab · Esc";
            }
        }

        void ShowHud(bool show)
        {
            if (_hudRoot != null) _hudRoot.SetActive(show);
            if (show) UpdateHud();
        }

        void EnableWorldCamera(bool on)
        {
            if (_worldCam != null) _worldCam.enabled = on;
            foreach (var cam in FindObjectsOfType<Camera>())
            {
                if (cam == _worldCam) continue;
                if (on) cam.enabled = false;
                else if (cam.GetComponent<AudioListener>() != null || cam.CompareTag("MainCamera"))
                    cam.enabled = true;
            }

            if (!on)
            {
                foreach (var al in FindObjectsOfType<AudioListener>())
                    al.enabled = true;
            }
        }

        void LateUpdate()
        {
            if (!IsActive || _player == null || _worldCam == null) return;
            if (!_player.gameObject.activeInHierarchy) return;
            var p = _player.transform.position;
            _worldCam.transform.position = new Vector3(p.x, p.y, -10f);
        }
    }
}
