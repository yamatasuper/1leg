using NinetyMinutes.Match;
using NinetyMinutes.Narrative;
using NinetyMinutes.Save;
using NinetyMinutes.Stats;
using NinetyMinutes.UI;
using NinetyMinutes.World;
using UnityEngine;

namespace NinetyMinutes.Core
{
    public enum SessionPhase
    {
        MainMenu = 0,
        GameplayStub = 1 // used as full gameplay from Sprint 1
    }

    public enum GraphicsPreset
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public sealed class GameSettings
    {
        const string KeyPreset = "nm_graphics_preset";
        const string KeyFps = "nm_fps_limit";
        const string KeyFullscreen = "nm_fullscreen";
        const string KeyLang = "nm_language";
        const string KeyMaster = "nm_vol_master";
        const string KeyMusic = "nm_vol_music";
        const string KeySfx = "nm_vol_sfx";
        const string KeyVoice = "nm_vol_voice";
        const string KeyCrowd = "nm_vol_crowd";

        public GraphicsPreset GraphicsPreset = GraphicsPreset.Medium;
        public int FpsLimit = 30;
        public bool Fullscreen = true;
        public string Language = "ru";
        public float VolMaster = 1f;
        public float VolMusic = 1f;
        public float VolSfx = 1f;
        public float VolVoice = 1f;
        public float VolCrowd = 1f;

        public void Load()
        {
            GraphicsPreset = (GraphicsPreset)PlayerPrefs.GetInt(KeyPreset, (int)GraphicsPreset.Medium);
            FpsLimit = PlayerPrefs.GetInt(KeyFps, 30);
            Fullscreen = PlayerPrefs.GetInt(KeyFullscreen, 1) == 1;
            Language = PlayerPrefs.GetString(KeyLang, "ru");
            VolMaster = PlayerPrefs.GetFloat(KeyMaster, 1f);
            VolMusic = PlayerPrefs.GetFloat(KeyMusic, 1f);
            VolSfx = PlayerPrefs.GetFloat(KeySfx, 1f);
            VolVoice = PlayerPrefs.GetFloat(KeyVoice, 1f);
            VolCrowd = PlayerPrefs.GetFloat(KeyCrowd, 1f);
            ApplyRuntime();
        }

        public void Save()
        {
            PlayerPrefs.SetInt(KeyPreset, (int)GraphicsPreset);
            PlayerPrefs.SetInt(KeyFps, FpsLimit);
            PlayerPrefs.SetInt(KeyFullscreen, Fullscreen ? 1 : 0);
            PlayerPrefs.SetString(KeyLang, Language);
            PlayerPrefs.SetFloat(KeyMaster, VolMaster);
            PlayerPrefs.SetFloat(KeyMusic, VolMusic);
            PlayerPrefs.SetFloat(KeySfx, VolSfx);
            PlayerPrefs.SetFloat(KeyVoice, VolVoice);
            PlayerPrefs.SetFloat(KeyCrowd, VolCrowd);
            PlayerPrefs.Save();
            ApplyRuntime();
        }

        public void ApplyRuntime()
        {
            Application.targetFrameRate = FpsLimit;
            Screen.fullScreen = Fullscreen;
            AudioListener.volume = VolMaster;
            QualitySettings.SetQualityLevel(GraphicsPreset == GraphicsPreset.Low ? 0 :
                GraphicsPreset == GraphicsPreset.Medium ? Mathf.Clamp(QualitySettings.names.Length / 2, 0, QualitySettings.names.Length - 1) :
                QualitySettings.names.Length - 1, true);
        }
    }

    public sealed class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        public GameSettings Settings { get; } = new GameSettings();
        public SessionPhase Phase { get; private set; } = SessionPhase.MainMenu;
        public float PlaytimeSec { get; private set; }
        public bool CampaignStarted { get; private set; }
        public string ActId { get; set; } = "menu";
        public string LocationId { get; set; } = "";
        public string TimeMode { get; set; } = "menu";

        public event System.Action PhaseChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Settings.Load();
        }

        void Update()
        {
            if (Phase == SessionPhase.GameplayStub)
                PlaytimeSec += Time.unscaledDeltaTime;
        }

        public void EnterMainMenu()
        {
            WorldController.Instance?.StopWorld();
            Phase = SessionPhase.MainMenu;
            Time.timeScale = 1f;
            PhaseChanged?.Invoke();
        }

        public void StartNewCampaign(bool wipeSaves)
        {
            if (wipeSaves && SaveService.Instance != null)
                SaveService.Instance.DeleteAllForNewGame();

            SoftStatsService.Instance?.ResetForNewRun();
            ChoiceScoreBridge.Instance?.ResetForNewRun();
            EndingsService.Instance?.ResetForNewRun();
            SliceDirector.Instance?.ResetForNewRun();
            Dialogue.SliceDialogues.Flags.Clear();

            CampaignStarted = true;
            PlaytimeSec = 0f;
            ActId = "Intro";
            LocationId = "loc_locker";
            TimeMode = "match";
            Phase = SessionPhase.GameplayStub;
            Time.timeScale = 1f;
            PhaseChanged?.Invoke();

            if (SaveService.Instance != null)
                SaveService.Instance.TrySaveAuto("new_game", BuildPayload(), out _);

            SliceDirector.Instance?.BeginNewRun();
        }

        public void ContinueFromPayload(SavePayload payload)
        {
            if (payload == null) return;
            CampaignStarted = true;
            PlaytimeSec = payload.meta != null ? payload.meta.playtimeSec : 0f;
            ActId = payload.meta != null && !string.IsNullOrEmpty(payload.meta.actId)
                ? payload.meta.actId
                : "training";
            LocationId = string.IsNullOrEmpty(payload.meta?.locationId) ? "loc_locker" : payload.meta.locationId;
            TimeMode = payload.meta?.timeMode ?? "past";

            if (SoftStatsService.Instance != null)
            {
                var s = SoftStatsService.Instance.State;
                s.Morale = payload.softMorale;
                s.Energy = payload.softEnergy;
                s.Strength = payload.softStrength;
                s.Focus = payload.softFocus;
                s.Pain = payload.softPain;
                s.Anxiety = payload.softAnxiety;
                s.ClampAll();
            }

            ChoiceScoreBridge.Instance?.Restore(
                payload.formPulse,
                payload.goalsFor,
                payload.goalsAgainst,
                payload.matchMinute > 0 ? payload.matchMinute : 1,
                payload.goalsEventsThisHalf);

            Dialogue.SliceDialogues.Flags.Clear();
            if (!string.IsNullOrEmpty(payload.flagsCsv))
            {
                foreach (var f in payload.flagsCsv.Split(','))
                {
                    var t = f.Trim();
                    if (!string.IsNullOrEmpty(t))
                        Dialogue.SliceDialogues.Flags.Add(t);
                }
            }

            if (System.Enum.TryParse(payload.endingRoute, out EndingRoute er))
                EndingsService.Instance?.Restore(er, payload.lifeScore);
            else
                EndingsService.Instance?.ResetForNewRun();

            Phase = SessionPhase.GameplayStub;
            Time.timeScale = 1f;
            PhaseChanged?.Invoke();

            var spawn = new Vector2(payload.playerX, payload.playerY);
            if (Mathf.Approximately(spawn.x, 0f) && Mathf.Approximately(spawn.y, 0f))
                spawn = new Vector2(0f, -1.5f);

            if (!System.Enum.TryParse(payload.slicePhase, out SlicePhase slicePhase))
                slicePhase = SlicePhase.Training;

            if (slicePhase == SlicePhase.Intro || slicePhase == SlicePhase.None)
            {
                SliceDirector.Instance?.BeginNewRun();
            }
            else if (slicePhase >= SlicePhase.EndingCard)
            {
                WorldController.Instance?.StartOrResumeWorld("loc_locker", spawn);
                SliceDirector.Instance?.ResumeFromSave(slicePhase);
            }
            else
            {
                var loc = slicePhase == SlicePhase.StreetLife || slicePhase == SlicePhase.Match
                    ? "loc_street"
                    : LocationId;
                WorldController.Instance?.StartOrResumeWorld(loc, spawn);
                SliceDirector.Instance?.ResumeFromSave(slicePhase);
            }
        }

        public SavePayload BuildPayload()
        {
            var playerPos = Vector2.zero;
            var player = FindObjectOfType<PlayerController>();
            if (player != null) playerPos = player.LocalGroundPos;

            if (WorldController.Instance != null)
                LocationId = WorldController.Instance.CurrentLocationId;

            var soft = SoftStatsService.Instance != null ? SoftStatsService.Instance.State : new SoftStatsState();
            var bridge = ChoiceScoreBridge.Instance;
            var flags = Dialogue.SliceDialogues.Flags;
            var flagsCsv = flags.Count == 0 ? "" : string.Join(",", flags);
            var gf = bridge != null ? bridge.Score.GoalsFor : 0;
            var ga = bridge != null ? bridge.Score.GoalsAgainst : 0;
            var minute = bridge != null ? bridge.MatchMinute : 1;
            var slice = SliceDirector.Instance != null ? SliceDirector.Instance.Phase.ToString() : "None";
            var ending = EndingsService.Instance != null ? EndingsService.Instance.LockedRoute.ToString() : "None";
            var life = EndingsService.Instance != null ? EndingsService.Instance.LastLifeScore : 0f;
            var endingLocked = EndingsService.Instance != null && EndingsService.Instance.IsLocked;

            return new SavePayload
            {
                campaignStarted = CampaignStarted,
                sessionPhase = Phase.ToString(),
                heroName = "Бардин",
                opponentName = bridge != null ? bridge.OpponentName : "Прибой",
                notes = "sprint3",
                softMorale = soft.Morale,
                softEnergy = soft.Energy,
                softStrength = soft.Strength,
                softFocus = soft.Focus,
                softPain = soft.Pain,
                softAnxiety = soft.Anxiety,
                playerX = playerPos.x,
                playerY = playerPos.y,
                formPulse = bridge != null ? bridge.FormPulse : 0f,
                goalsFor = gf,
                goalsAgainst = ga,
                matchMinute = minute,
                goalsEventsThisHalf = bridge != null ? bridge.GoalsEventsThisHalf : 0,
                flagsCsv = flagsCsv,
                slicePhase = slice,
                endingRoute = ending,
                lifeScore = life,
                meta = new SaveMeta
                {
                    playtimeSec = PlaytimeSec,
                    actId = ActId,
                    locationId = LocationId,
                    timeMode = TimeMode,
                    matchMinute = minute,
                    goalsFor = gf,
                    goalsAgainst = ga,
                    endingLocked = endingLocked,
                    summaryLabel = $"Игра · {slice} · {gf}:{ga}"
                }
            };
        }
    }
}
