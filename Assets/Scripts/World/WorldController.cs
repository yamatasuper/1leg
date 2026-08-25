using NinetyMinutes.Core;
using NinetyMinutes.Dialogue;
using NinetyMinutes.Narrative;
using NinetyMinutes.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        GameObject _npcCoachLocker;
        GameObject _npcSkipTraining;
        GameObject _npcGlockStreet;
        GameObject _npcSokolStreet;
        GameObject _npcSelfThought;
        DoorInteractable _doorToStreet;
        DoorInteractable _doorToLocker;

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
            if (_persistentRoot != null) return;

            LoadOrBuild(WorldSceneFactory.PersistentScene, WorldSceneFactory.BuildPersistent);
            LoadOrBuild(WorldSceneFactory.LockerScene, WorldSceneFactory.BuildLocker);
            LoadOrBuild(WorldSceneFactory.StreetScene, WorldSceneFactory.BuildStreet);

            BindSceneObjects();
            BuildHud();

            if (_locker != null) _locker.SetActive(true);
            if (_street != null) _street.SetActive(false);
        }

        static void LoadOrBuild(string sceneName, System.Func<GameObject> fallback)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded) return;

            var inBuild = false;
            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrEmpty(path)) continue;
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName)
                {
                    inBuild = true;
                    break;
                }
            }

            if (inBuild)
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                return;
            }

            Debug.LogWarning($"[90 минут] Scene '{sceneName}' not in Build Settings — building in memory. Menu: 90 минут / Bake World Scenes.");
            fallback();
        }

        void BindSceneObjects()
        {
            _persistentRoot = GameObject.Find("World_Persistent");
            _locker = GameObject.Find("loc_locker");
            _street = GameObject.Find("loc_street");

            if (_persistentRoot != null)
            {
                _sun = FindChild<Light>(_persistentRoot.transform, "Sun");
                var camGo = FindChild(_persistentRoot.transform, "WorldCamera");
                if (camGo != null)
                {
                    _worldCam = camGo.GetComponent<Camera>();
                    _camRig = camGo.GetComponent<WorldCameraRig>();
                }

                var playerGo = FindChild(_persistentRoot.transform, "Player");
                if (playerGo != null)
                {
                    _player = playerGo.GetComponent<PlayerController>();
                    if (_player != null) _player.CameraRig = _camRig;
                    if (_camRig != null) _camRig.Target = playerGo.transform;
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
            if (root.name == name) return root.gameObject;
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
                    ? new Color(0.95f, 0.9f, 0.8f)
                    : new Color(1f, 0.97f, 0.9f);
            }

            if (_worldCam != null)
            {
                _worldCam.backgroundColor = locker
                    ? new Color(0.22f, 0.23f, 0.24f)
                    : new Color(0.58f, 0.66f, 0.72f);
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = locker
                ? new Color(0.28f, 0.27f, 0.24f)
                : new Color(0.62f, 0.7f, 0.76f);
            RenderSettings.fogStartDistance = locker ? 12f : 16f;
            RenderSettings.fogEndDistance = locker ? 28f : 48f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = locker
                ? new Color(0.42f, 0.4f, 0.36f)
                : new Color(0.62f, 0.66f, 0.7f);
        }

        void BuildHud()
        {
            if (_hudRoot != null) return;
            var canvas = UiFactory.CreateCanvas("WorldHudCanvas", 150);
            DontDestroyOnLoad(canvas.gameObject);
            _hudRoot = canvas.gameObject;
            var panel = UiFactory.Box(canvas.transform, "HintBox", new Vector2(0, 460), new Vector2(1200, 70),
                new Color(0.05f, 0.06f, 0.08f, 0.75f));
            _hudHint = UiFactory.Label(panel, "Hint", "", 22, TextAnchor.MiddleCenter, Color.white);
            ShowHud(false);
        }

        void UpdateHud()
        {
            if (_hudHint == null) return;
            var loc = CurrentLocationId == "loc_locker" ? "Раздевалка «Торпедо»" : "Бровка";
            var controls = "WASD · ПКМ/Q/Z камера · E · Tab · Esc";
            if (SliceDirector.Instance != null && SliceDirector.Instance.Phase == SlicePhase.StreetLife)
            {
                var left = 0;
                foreach (var f in SliceDirector.StreetDialogueFlags)
                    if (!SliceDialogues.Flags.Contains(f)) left++;
                _hudHint.text = $"{loc} · поговори со всеми ({3 - left}/3) · потом матч · {controls}";
            }
            else
            {
                var phase = SliceDirector.Instance != null ? SliceDirector.Instance.Phase.ToString() : "";
                _hudHint.text = $"{loc} · {phase} · {controls}";
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
            if (_camRig != null) _camRig.InputEnabled = on;
            if (_sun != null) _sun.enabled = on;

            foreach (var cam in FindObjectsOfType<Camera>())
            {
                if (cam == _worldCam) continue;
                if (on) cam.enabled = false;
                else if (cam.GetComponent<AudioListener>() != null || cam.CompareTag("MainCamera"))
                    cam.enabled = true;
            }

            if (on)
            {
                ApplyLocationAtmosphere(CurrentLocationId == "loc_locker");
                if (_worldCam != null)
                {
                    foreach (var al in FindObjectsOfType<AudioListener>())
                        al.enabled = al.gameObject == _worldCam.gameObject;
                }

                if (_camRig != null) _camRig.Snap();
            }
            else
            {
                RenderSettings.fog = false;
                foreach (var al in FindObjectsOfType<AudioListener>())
                    al.enabled = true;
            }
        }
    }
}
