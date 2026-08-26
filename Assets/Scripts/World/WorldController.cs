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

        GameObject _persistentRoot;
        GameObject _locker;
        GameObject _street;
        PlayerController _player;
        Camera _worldCam;
        WorldCameraRig _camRig;
        Light _sun;
        Light _lockerLamp;
        Text _hudHint;
        GameObject _hudRoot;
        string _focusPrompt;

        GameObject _npcCoachLocker;
        GameObject _npcSkipTraining;
        GameObject _npcGlockStreet;
        GameObject _npcSokolStreet;
        GameObject _npcSelfThought;
        DoorInteractable _doorToStreet;
        DoorInteractable _doorToLocker;

        public Vector3 ActiveOrigin
        {
            get
            {
                var loc = CurrentLocationId == "loc_locker" ? _locker : _street;
                return loc != null ? loc.transform.position : Vector3.zero;
            }
        }

        public string CurrentLocationId { get; private set; } = "loc_locker";
        public bool IsActive => _persistentRoot != null && _persistentRoot.activeSelf;

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
            EnsureWorldLoaded();
            if (_persistentRoot != null) _persistentRoot.SetActive(true);
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
            if (_persistentRoot != null) _persistentRoot.SetActive(false);
            if (_locker != null) _locker.SetActive(false);
            if (_street != null) _street.SetActive(false);
        }

        public void SuspendForMatch(bool suspend)
        {
            if (_player != null) _player.InputLocked = suspend;
            ShowHud(!suspend && IsActive);
            EnableWorldCamera(!suspend && _persistentRoot != null && _persistentRoot.activeSelf);
        }

        public void TravelTo(string locationId, Vector2? spawn = null)
        {
            EnsureWorldLoaded();
            CurrentLocationId = locationId;
            if (GameSession.Instance != null)
                GameSession.Instance.LocationId = locationId;

            var toLocker = locationId == "loc_locker";
            if (_locker != null) _locker.SetActive(toLocker);
            if (_street != null) _street.SetActive(!toLocker);
            if (_lockerLamp != null) _lockerLamp.enabled = toLocker;
            ApplyLocationAtmosphere(toLocker);

            if (_player != null)
            {
                _player.gameObject.SetActive(true);
                _player.Place(spawn ?? new Vector2(0f, -2.2f));
            }

            if (_camRig != null)
            {
                _camRig.Yaw = 0f;
                _camRig.Snap();
            }

            UpdateHud();
        }

        public void RefreshSpine(SlicePhase phase)
        {
            EnsureWorldLoaded();

            var street = phase == SlicePhase.StreetLife;
            SetActive(_npcCoachLocker, phase == SlicePhase.Training);
            SetActive(_npcSkipTraining, phase == SlicePhase.Training && !SliceDialogues.Flags.Contains("training_done"));
            SetActive(_npcGlockStreet, street && !SliceDialogues.Flags.Contains("street_glock_done"));
            SetActive(_npcSokolStreet, street && !SliceDialogues.Flags.Contains("street_sokol_done"));
            SetActive(_npcSelfThought, street && !SliceDialogues.Flags.Contains("street_self_done"));

            if (_doorToStreet != null)
            {
                _doorToStreet.RequireFlag = null;
                _doorToStreet.LockedLine = null;
                _doorToStreet.Prompt = "E — выйти на бровку";
            }

            if (_doorToLocker != null)
            {
                _doorToLocker.RequireFlag = null;
                _doorToLocker.LockedLine = null;
            }

            BindNamedNpcs();
            UpdateHud();
        }

        static void SetActive(GameObject go, bool on)
        {
            if (go != null) go.SetActive(on);
        }

        void EnsureWorldLoaded()
        {
            if (_persistentRoot != null && _locker != null && _street != null) return;

            // Additive SceneManager.LoadScene only completes at the end of the frame, so the
            // world is spawned straight from the location prefabs to keep binding synchronous.
            if (FindNamed("World_Persistent") == null) WorldSceneFactory.BuildPersistent();
            if (FindLocation("loc_locker") == null) WorldSceneFactory.BuildLocker();
            if (FindLocation("loc_street") == null) WorldSceneFactory.BuildStreet();

            BindSceneObjects();
            BuildHud();

            if (_locker != null) _locker.SetActive(true);
            if (_street != null) _street.SetActive(false);
        }

        void BindSceneObjects()
        {
            _persistentRoot = FindNamed("World_Persistent");
            _locker = FindLocation("loc_locker");
            _street = FindLocation("loc_street");

            var rig = Object.FindObjectOfType<WorldCameraRig>(true);
            if (rig != null)
            {
                _camRig = rig;
                _worldCam = rig.GetComponent<Camera>();
                if (_persistentRoot == null)
                    _persistentRoot = rig.transform.root.gameObject;
            }

            if (_worldCam == null)
            {
                var camGo = FindNamed("WorldCamera");
                if (camGo != null)
                {
                    _worldCam = camGo.GetComponent<Camera>();
                    _camRig = camGo.GetComponent<WorldCameraRig>() ?? camGo.AddComponent<WorldCameraRig>();
                }
            }

            if (_worldCam == null)
                CreateEmergencyCamera();

            WorldGeometryFix.Stabilize(_locker, _street);
            WorldGeometryFix.OpenExits(_locker, _street);

            if (_sun == null && _persistentRoot != null)
                _sun = FindChild<Light>(_persistentRoot.transform, "Sun");

            var playerGo = Object.FindObjectOfType<PlayerController>(true);
            if (playerGo != null)
            {
                _player = playerGo;
                if (_player.CameraRig == null) _player.CameraRig = _camRig;
                if (_camRig != null) _camRig.Target = _player.transform;
            }
            else if (_persistentRoot != null)
            {
                var p = FindChild(_persistentRoot.transform, "Player");
                if (p != null)
                {
                    _player = p.GetComponent<PlayerController>() ?? p.AddComponent<PlayerController>();
                    _player.CameraRig = _camRig;
                    if (_camRig != null) _camRig.Target = p.transform;
                }
            }

            if (_locker != null)
            {
                _lockerLamp = FindChild<Light>(_locker.transform, "LockerLamp");
                _npcCoachLocker = FindChild(_locker.transform, "npc_coach");
                _npcSkipTraining = FindChild(_locker.transform, "skip_training");
                var door = FindChild(_locker.transform, "door_to_street");
                if (door != null) _doorToStreet = door.GetComponent<DoorInteractable>();
            }

            if (_street != null)
            {
                _npcGlockStreet = FindChild(_street.transform, "npc_glock");
                _npcSokolStreet = FindChild(_street.transform, "npc_sokol");
                _npcSelfThought = FindChild(_street.transform, "self_thought");
                var door = FindChild(_street.transform, "door_to_locker");
                if (door != null) _doorToLocker = door.GetComponent<DoorInteractable>();
            }

            BindNamedNpcs();
            CharacterVisualFix.Dress(_player, _locker, _street);
        }

        void CreateEmergencyCamera()
        {
            var camGo = new GameObject("WorldCamera");
            if (_persistentRoot != null)
                camGo.transform.SetParent(_persistentRoot.transform, false);
            camGo.tag = "MainCamera";
            _worldCam = camGo.AddComponent<Camera>();
            _worldCam.orthographic = false;
            _worldCam.fieldOfView = 55f;
            _worldCam.nearClipPlane = 0.4f;
            _worldCam.farClipPlane = 70f;
            _worldCam.clearFlags = CameraClearFlags.SolidColor;
            _worldCam.backgroundColor = new Color(0.42f, 0.4f, 0.34f);
            _worldCam.depth = 20;
            _worldCam.targetDisplay = 0;
            if (camGo.GetComponent<AudioListener>() == null)
                camGo.AddComponent<AudioListener>();
            _camRig = camGo.AddComponent<WorldCameraRig>();
            if (_persistentRoot == null)
                Object.DontDestroyOnLoad(camGo);
        }

        static GameObject FindLocation(string locationId)
        {
            var named = FindNamed(locationId);
            if (named != null) return named;
            foreach (var loc in Object.FindObjectsOfType<LocationScene>(true))
            {
                if (loc != null && loc.LocationId == locationId)
                    return loc.gameObject;
            }

            return null;
        }

        static GameObject FindNamed(string name)
        {
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
            {
                if (NamesEqual(t.name, name))
                    return t.gameObject;
            }

            return null;
        }

        static bool NamesEqual(string actual, string expected)
        {
            if (string.IsNullOrEmpty(actual) || string.IsNullOrEmpty(expected)) return false;
            var a = actual.Replace("(Clone)", "").Trim();
            return string.Equals(a, expected, System.StringComparison.OrdinalIgnoreCase);
        }

        void BindNamedNpcs()
        {
            BindNpc(_npcCoachLocker);
            BindNpc(_npcSkipTraining);
            BindNpc(_npcGlockStreet);
            BindNpc(_npcSokolStreet);
            BindNpc(_npcSelfThought);
        }

        static void BindNpc(GameObject go)
        {
            if (go == null) return;
            WorldNpcBinder.Bind(go.GetComponent<NpcInteractable>());
        }

        static GameObject FindChild(Transform root, string name)
        {
            if (root == null) return null;
            if (NamesEqual(root.name, name)) return root.gameObject;
            foreach (Transform child in root)
            {
                var found = FindChild(child, name);
                if (found != null) return found;
            }

            return null;
        }

        static T FindChild<T>(Transform root, string name) where T : Component
        {
            var go = FindChild(root, name);
            return go != null ? go.GetComponent<T>() : null;
        }

        void ApplyLocationAtmosphere(bool locker)
        {
            if (_sun != null)
            {
                _sun.intensity = locker ? 0.55f : 1.2f;
                _sun.color = locker
                    ? new Color(1f, 0.78f, 0.48f)
                    : new Color(1f, 0.82f, 0.55f);
            }

            if (_worldCam != null)
            {
                _worldCam.backgroundColor = locker
                    ? new Color(0.28f, 0.24f, 0.18f)
                    : new Color(0.45f, 0.48f, 0.42f);
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = locker
                ? new Color(0.32f, 0.26f, 0.18f)
                : new Color(0.48f, 0.5f, 0.42f);
            RenderSettings.fogStartDistance = locker ? 10f : 14f;
            RenderSettings.fogEndDistance = locker ? 26f : 44f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = locker
                ? new Color(0.38f, 0.34f, 0.28f)
                : new Color(0.42f, 0.48f, 0.46f);
        }

        void BuildHud()
        {
            if (_hudRoot != null) return;
            var canvas = UiFactory.CreateCanvas("WorldHudCanvas", 150);
            DontDestroyOnLoad(canvas.gameObject);
            _hudRoot = canvas.gameObject;
            var panel = UiFactory.Box(canvas.transform, "HintBox", new Vector2(0, 470), new Vector2(1280, 78),
                new Color(0.08f, 0.07f, 0.05f, 0.82f));
            var outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.78f, 0.58f, 0.22f, 0.45f);
            outline.effectDistance = new Vector2(1, -1);
            _hudHint = UiFactory.Label(panel, "Hint", "", 22, TextAnchor.MiddleCenter, new Color(0.96f, 0.93f, 0.86f));
            ShowHud(false);
        }

        public void SetFocusPrompt(string prompt)
        {
            if (_focusPrompt == prompt) return;
            _focusPrompt = prompt;
            UpdateHud();
        }

        void UpdateHud()
        {
            if (_hudHint == null) return;
            var loc = CurrentLocationId == "loc_locker" ? "Раздевалка «Торпедо»" : "Бровка";
            var focus = string.IsNullOrEmpty(_focusPrompt) ? "" : $" · {_focusPrompt}";
            if (SliceDirector.Instance != null && SliceDirector.Instance.Phase == SlicePhase.Training)
            {
                _hudHint.text = $"{loc} · тренер или дверь налево (A){focus}";
                return;
            }

            if (SliceDirector.Instance != null && SliceDirector.Instance.Phase == SlicePhase.StreetLife)
            {
                var left = 0;
                foreach (var f in SliceDirector.StreetDialogueFlags)
                    if (!SliceDialogues.Flags.Contains(f)) left++;
                _hudHint.text = $"{loc} · поговори со всеми ({3 - left}/3){focus}";
                return;
            }

            var phase = SliceDirector.Instance != null ? SliceDirector.Instance.Phase.ToString() : "";
            _hudHint.text = $"{loc} · {phase}{focus}";
        }

        void ShowHud(bool show)
        {
            if (_hudRoot != null) _hudRoot.SetActive(show);
            if (show) UpdateHud();
        }

        void EnableWorldCamera(bool on)
        {
            if (on)
            {
                if (_worldCam != null)
                {
                    ActivateHierarchy(_worldCam.gameObject);
                    _worldCam.enabled = true;
                }

                if (_worldCam == null || !_worldCam.gameObject.activeInHierarchy)
                    CreateEmergencyCamera();
            }

            if (_worldCam != null)
            {
                ActivateHierarchy(_worldCam.gameObject);
                _worldCam.enabled = on;
                _worldCam.targetTexture = null;
                _worldCam.targetDisplay = 0;
                _worldCam.depth = 20;
                _worldCam.nearClipPlane = Mathf.Max(_worldCam.nearClipPlane, 0.4f);
                _worldCam.farClipPlane = Mathf.Max(_worldCam.farClipPlane, 70f);
                if (!_worldCam.CompareTag("MainCamera"))
                    _worldCam.tag = "MainCamera";
            }

            if (_camRig != null) _camRig.InputEnabled = on;
            if (_sun != null) _sun.enabled = on;

            var worldLive = on && _worldCam != null && _worldCam.enabled && _worldCam.gameObject.activeInHierarchy;
            foreach (var cam in FindObjectsOfType<Camera>(true))
            {
                if (cam == _worldCam) continue;
                if (worldLive && cam.targetDisplay == 0 && cam.targetTexture == null)
                    cam.enabled = false;
                else if (!on && (cam.CompareTag("MainCamera") || cam.GetComponent<AudioListener>() != null))
                    cam.enabled = true;
            }

            if (!worldLive && on)
            {
                foreach (var cam in FindObjectsOfType<Camera>(true))
                {
                    if (cam.targetDisplay != 0 || cam.targetTexture != null) continue;
                    ActivateHierarchy(cam.gameObject);
                    cam.enabled = true;
                }
            }

            if (on)
            {
                ApplyLocationAtmosphere(CurrentLocationId == "loc_locker");
                if (_worldCam != null)
                {
                    foreach (var al in FindObjectsOfType<AudioListener>(true))
                        al.enabled = al.gameObject == _worldCam.gameObject;
                }

                if (_camRig != null)
                {
                    if (_player != null) _camRig.Target = _player.transform;
                    _camRig.Snap();
                }
            }
            else
            {
                RenderSettings.fog = false;
                foreach (var al in FindObjectsOfType<AudioListener>(true))
                    al.enabled = true;
            }
        }

        static void ActivateHierarchy(GameObject go)
        {
            if (go == null) return;
            var t = go.transform;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                t = t.parent;
            }
        }
    }
}
