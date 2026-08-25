using System;
using System.Collections.Generic;
using NinetyMinutes.Core;
using NinetyMinutes.Save;
using NinetyMinutes.World;
using UnityEngine;
using UnityEngine.UI;

namespace NinetyMinutes.UI
{
    public sealed class AppShellMenu : MonoBehaviour
    {
        public static AppShellMenu Instance { get; private set; }

        static readonly Color Bg = new Color(0.02f, 0.03f, 0.04f, 0.97f);
        static readonly Color Bezel = new Color(0.1f, 0.12f, 0.16f, 1f);
        static readonly Color Accent = new Color(0.95f, 0.92f, 0.35f, 1f);
        static readonly Color TextCol = new Color(1f, 1f, 0.95f, 1f);
        static readonly Color Dim = new Color(0.7f, 0.72f, 0.65f, 1f);

        Canvas _canvas;
        GameObject _main;
        GameObject _pause;
        GameObject _settings;
        GameObject _slots;
        GameObject _modal;
        GameObject _gameplayStub;
        Text _status;
        Text _modalText;
        Action _modalConfirm;
        bool _slotsAreLoad;
        bool _pauseOpen;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            UiFactory.EnsureEventSystem();
            BuildUi();
            if (GameSession.Instance != null)
                GameSession.Instance.PhaseChanged += RefreshForPhase;
            RefreshForPhase();
        }

        void OnDestroy()
        {
            if (GameSession.Instance != null)
                GameSession.Instance.PhaseChanged -= RefreshForPhase;
        }

        void Update()
        {
            if (GameSession.Instance == null) return;
            if (GameSession.Instance.Phase != SessionPhase.GameplayStub) return;
            if (_modal != null && _modal.activeSelf) return;
            if (_settings != null && _settings.activeSelf) return;
            if (_slots != null && _slots.activeSelf) return;

            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        // Pause is allowed during match beat (menu only); save remains blocked by SaveService.

        void BuildUi()
        {
            _canvas = UiFactory.CreateCanvas("AppShellCanvas", 200);
            DontDestroyOnLoad(_canvas.gameObject);

            BuildMain();
            BuildPause();
            BuildSettings();
            BuildSlots();
            BuildModal();
            BuildGameplayStub();
        }

        void BuildMain()
        {
            _main = UiFactory.Panel(_canvas.transform, "MainMenu", Bg).gameObject;
            var frame = UiFactory.Box(_main.transform, "TVFrame", Vector2.zero, new Vector2(900, 720), Bezel);
            UiFactory.Title(frame, "OnAir", "● ON AIR   90 МИНУТ", 22, new Vector2(0, -24), Accent);
            UiFactory.Title(frame, "Title", "90 МИНУТ", 64, new Vector2(0, -70), TextCol);
            UiFactory.Title(frame, "Sub", "Последний матч · 3D", 22, new Vector2(0, -150), Dim);

            float y = -20;
            UiFactory.Button(frame, "BtnNew", "Новая игра", new Vector2(0, y), new Vector2(420, 64), OnNewGame);
            y -= 80;
            var cont = UiFactory.Button(frame, "BtnContinue", "Продолжить", new Vector2(0, y), new Vector2(420, 64), OnContinue);
            cont.name = "BtnContinue";
            y -= 80;
            UiFactory.Button(frame, "BtnSettings", "Настройки", new Vector2(0, y), new Vector2(420, 64), () => ShowSettings(fromPause: false));
            y -= 80;
            UiFactory.Button(frame, "BtnQuit", "Выход", new Vector2(0, y), new Vector2(420, 64), OnQuit);

            _status = UiFactory.Footer(frame, "Status", "", 20, Dim);
        }

        void BuildPause()
        {
            _pause = UiFactory.Panel(_canvas.transform, "PauseMenu", new Color(0, 0, 0, 0.72f)).gameObject;
            var frame = UiFactory.Box(_pause.transform, "TVFrame", Vector2.zero, new Vector2(760, 680), Bezel);
            UiFactory.Title(frame, "Title", "ПАУЗА", 48, new Vector2(0, -40), TextCol);

            float y = 80;
            UiFactory.Button(frame, "Resume", "Продолжить", new Vector2(0, y), new Vector2(400, 58), () => SetPause(false));
            y -= 72;
            UiFactory.Button(frame, "Save", "Сохранить", new Vector2(0, y), new Vector2(400, 58), () => OpenSlots(load: false));
            y -= 72;
            UiFactory.Button(frame, "Load", "Загрузить", new Vector2(0, y), new Vector2(400, 58), () => OpenSlots(load: true));
            y -= 72;
            UiFactory.Button(frame, "Settings", "Настройки", new Vector2(0, y), new Vector2(400, 58), () => ShowSettings(fromPause: true));
            y -= 72;
            UiFactory.Button(frame, "ToTitle", "В главное меню", new Vector2(0, y), new Vector2(400, 58), ConfirmToTitle);
            y -= 72;
            UiFactory.Button(frame, "Quit", "Выход", new Vector2(0, y), new Vector2(400, 58), ConfirmQuit);
            _pause.SetActive(false);
        }

        void BuildSettings()
        {
            _settings = UiFactory.Panel(_canvas.transform, "Settings", new Color(0, 0, 0, 0.8f)).gameObject;
            var frame = UiFactory.Box(_settings.transform, "TVFrame", Vector2.zero, new Vector2(820, 780), Bezel);
            UiFactory.Title(frame, "Title", "НАСТРОЙКИ", 42, new Vector2(0, -36), TextCol);

            float y = 120;
            AddCycleButton(frame, "Графика", () =>
            {
                var s = GameSession.Instance.Settings;
                s.GraphicsPreset = (GraphicsPreset)(((int)s.GraphicsPreset + 1) % 3);
                s.Save();
                return $"Графика · {PresetName(s.GraphicsPreset)}";
            }, ref y);

            AddCycleButton(frame, "FPS", () =>
            {
                var s = GameSession.Instance.Settings;
                s.FpsLimit = s.FpsLimit == 30 ? 60 : 30;
                s.Save();
                return $"Ограничение FPS · {s.FpsLimit}";
            }, ref y);

            AddCycleButton(frame, "Экран", () =>
            {
                var s = GameSession.Instance.Settings;
                s.Fullscreen = !s.Fullscreen;
                s.Save();
                return $"Полный экран · {(s.Fullscreen ? "ВКЛ" : "ВЫКЛ")}";
            }, ref y);

            AddCycleButton(frame, "Язык", () =>
            {
                var s = GameSession.Instance.Settings;
                s.Language = s.Language == "ru" ? "en" : "ru";
                s.Save();
                return $"Язык · {s.Language.ToUpperInvariant()}";
            }, ref y);

            AddCycleButton(frame, "Master", () => CycleVolume(v => GameSession.Instance.Settings.VolMaster = v, () => GameSession.Instance.Settings.VolMaster, "Общая"), ref y);
            AddCycleButton(frame, "Music", () => CycleVolume(v => GameSession.Instance.Settings.VolMusic = v, () => GameSession.Instance.Settings.VolMusic, "Музыка"), ref y);
            AddCycleButton(frame, "Sfx", () => CycleVolume(v => GameSession.Instance.Settings.VolSfx = v, () => GameSession.Instance.Settings.VolSfx, "Эффекты"), ref y);
            AddCycleButton(frame, "Voice", () => CycleVolume(v => GameSession.Instance.Settings.VolVoice = v, () => GameSession.Instance.Settings.VolVoice, "Голос"), ref y);
            AddCycleButton(frame, "Crowd", () => CycleVolume(v => GameSession.Instance.Settings.VolCrowd = v, () => GameSession.Instance.Settings.VolCrowd, "Толпа"), ref y);

            UiFactory.Button(frame, "Back", "Назад", new Vector2(0, -320), new Vector2(360, 56), HideSettings);
            _settings.SetActive(false);
        }

        void AddCycleButton(Transform parent, string name, Func<string> cycle, ref float y)
        {
            var btn = UiFactory.Button(parent, name, RefreshSettingLabel(name), new Vector2(0, y), new Vector2(520, 52), null);
            btn.onClick.AddListener(() =>
            {
                var t = btn.GetComponentInChildren<Text>();
                if (t != null) t.text = cycle();
            });
            y -= 58;
        }

        string RefreshSettingLabel(string name)
        {
            var s = GameSession.Instance.Settings;
            switch (name)
            {
                case "Графика": return $"Графика · {PresetName(s.GraphicsPreset)}";
                case "FPS": return $"Ограничение FPS · {s.FpsLimit}";
                case "Экран": return $"Полный экран · {(s.Fullscreen ? "ВКЛ" : "ВЫКЛ")}";
                case "Язык": return $"Язык · {s.Language.ToUpperInvariant()}";
                case "Master": return $"Общая · {Mathf.RoundToInt(s.VolMaster * 100)}%";
                case "Music": return $"Музыка · {Mathf.RoundToInt(s.VolMusic * 100)}%";
                case "Sfx": return $"Эффекты · {Mathf.RoundToInt(s.VolSfx * 100)}%";
                case "Voice": return $"Голос · {Mathf.RoundToInt(s.VolVoice * 100)}%";
                case "Crowd": return $"Толпа · {Mathf.RoundToInt(s.VolCrowd * 100)}%";
                default: return name;
            }
        }

        string CycleVolume(Action<float> set, Func<float> get, string label)
        {
            var v = get();
            v += 0.25f;
            if (v > 1.01f) v = 0f;
            set(v);
            GameSession.Instance.Settings.Save();
            return $"{label} · {Mathf.RoundToInt(get() * 100)}%";
        }

        void BuildSlots()
        {
            _slots = UiFactory.Panel(_canvas.transform, "Slots", new Color(0, 0, 0, 0.85f)).gameObject;
            var frame = UiFactory.Box(_slots.transform, "TVFrame", Vector2.zero, new Vector2(900, 820), Bezel);
            UiFactory.Title(frame, "Title", "СОХРАНЕНИЯ", 40, new Vector2(0, -36), TextCol);
            // Content rebuilt each open
            var list = new GameObject("List", typeof(RectTransform));
            list.transform.SetParent(frame, false);
            var rt = list.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, -20);
            rt.sizeDelta = new Vector2(760, 620);
            UiFactory.Button(frame, "Back", "Назад", new Vector2(0, -360), new Vector2(300, 52), () =>
            {
                _slots.SetActive(false);
                if (_pauseOpen) _pause.SetActive(true);
            });
            _slots.SetActive(false);
        }

        void BuildModal()
        {
            _modal = UiFactory.Panel(_canvas.transform, "Modal", new Color(0, 0, 0, 0.88f)).gameObject;
            var frame = UiFactory.Box(_modal.transform, "TVFrame", Vector2.zero, new Vector2(700, 320), Bezel);
            _modalText = UiFactory.Label(frame, "Text", "", 26, TextAnchor.MiddleCenter, TextCol);
            _modalText.rectTransform.offsetMin = new Vector2(30, 90);
            _modalText.rectTransform.offsetMax = new Vector2(-30, -70);
            UiFactory.Button(frame, "Ok", "OK", new Vector2(-120, -100), new Vector2(200, 52), () =>
            {
                _modal.SetActive(false);
                _modalConfirm?.Invoke();
                _modalConfirm = null;
            });
            UiFactory.Button(frame, "Cancel", "Отмена", new Vector2(120, -100), new Vector2(200, 52), () =>
            {
                _modal.SetActive(false);
                _modalConfirm = null;
            });
            _modal.SetActive(false);
        }

        void BuildGameplayStub()
        {
            _gameplayStub = UiFactory.Panel(_canvas.transform, "GameplayStub", new Color(0.07f, 0.09f, 0.11f, 1f)).gameObject;
            _gameplayStub.transform.SetAsFirstSibling();
            var frame = UiFactory.Box(_gameplayStub.transform, "Board", Vector2.zero, new Vector2(1000, 420), Bezel);
            UiFactory.Title(frame, "Title", "ИГРА · STUB", 44, new Vector2(0, -36), Accent);
            UiFactory.Label(frame, "Body",
                    "Sprint 0 готов.\n\nEsc — пауза (Сохранить / Загрузить / Настройки).\nНовая игра создаёт автосейв.\nСейвы: " +
                    Application.persistentDataPath + "/saves",
                    24, TextAnchor.MiddleCenter, TextCol)
                .rectTransform.offsetMin = new Vector2(40, 40);
            _gameplayStub.SetActive(false);
        }

        void RefreshForPhase()
        {
            var phase = GameSession.Instance != null ? GameSession.Instance.Phase : SessionPhase.MainMenu;
            var inGame = phase == SessionPhase.GameplayStub;
            _main.SetActive(!inGame);
            // WorldController owns the gameplay view from Sprint 1.
            if (_gameplayStub != null) _gameplayStub.SetActive(false);
            if (!inGame)
            {
                SetPause(false);
                _settings.SetActive(false);
                _slots.SetActive(false);
            }

            RefreshContinueButton();
            if (_status != null)
            {
                _status.text = SaveService.Instance != null && SaveService.Instance.HasAnySave()
                    ? "Есть сохранения"
                    : "Нет сохранений";
            }
        }

        void RefreshContinueButton()
        {
            var btn = _main.transform.Find("TVFrame/BtnContinue")?.GetComponent<Button>();
            if (btn == null) return;
            var has = SaveService.Instance != null && SaveService.Instance.HasAnySave();
            btn.interactable = has;
            var t = btn.GetComponentInChildren<Text>();
            if (t != null) t.color = has ? TextCol : Dim;
        }

        void OnNewGame()
        {
            if (SaveService.Instance != null && SaveService.Instance.HasAnySave())
            {
                ShowModal("Текущий прогресс будет перезаписан. Начать новую игру?", () =>
                {
                    GameSession.Instance.StartNewCampaign(wipeSaves: true);
                });
            }
            else
            {
                GameSession.Instance.StartNewCampaign(wipeSaves: false);
            }
        }

        void OnContinue()
        {
            if (SaveService.Instance == null || !SaveService.Instance.TryGetLatest(out var payload))
            {
                ShowModal("Нет сохранений.", null, cancelOnly: false);
                return;
            }

            GameSession.Instance.ContinueFromPayload(payload);
        }

        void OnQuit()
        {
            ShowModal("Выйти из игры?", () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        void ConfirmQuit() => OnQuit();

        void ConfirmToTitle()
        {
            ShowModal("Вернуться в главное меню?", () =>
            {
                SetPause(false);
                GameSession.Instance.EnterMainMenu();
            });
        }

        void TogglePause() => SetPause(!_pauseOpen);

        void SetPause(bool open)
        {
            _pauseOpen = open;
            _pause.SetActive(open);
            Time.timeScale = open ? 0f : 1f;
            var player = FindObjectOfType<PlayerController>();
            var matchPlaying = Match.MatchPresentation.Instance != null && Match.MatchPresentation.Instance.IsPlaying;
            if (player != null)
                player.InputLocked = open || matchPlaying;
            if (!open)
            {
                _settings.SetActive(false);
                _slots.SetActive(false);
            }
        }

        void ShowSettings(bool fromPause)
        {
            if (fromPause) _pause.SetActive(false);
            _main.SetActive(false);
            RefreshSettingsLabels();
            _settings.SetActive(true);
        }

        void HideSettings()
        {
            _settings.SetActive(false);
            if (GameSession.Instance.Phase == SessionPhase.MainMenu)
                _main.SetActive(true);
            else if (_pauseOpen)
                _pause.SetActive(true);
        }

        void RefreshSettingsLabels()
        {
            foreach (Transform child in _settings.transform.Find("TVFrame"))
            {
                var btn = child.GetComponent<Button>();
                if (btn == null) continue;
                var t = btn.GetComponentInChildren<Text>();
                if (t == null) continue;
                var refreshed = RefreshSettingLabel(child.name);
                if (!string.IsNullOrEmpty(refreshed) && child.name != "Back" && child.name != "Title")
                {
                    // only known setting buttons
                    if (child.name == "Графика" || child.name == "FPS" || child.name == "Экран" || child.name == "Язык" ||
                        child.name == "Master" || child.name == "Music" || child.name == "Sfx" || child.name == "Voice" || child.name == "Crowd")
                        t.text = refreshed;
                }
            }
        }

        void OpenSlots(bool load)
        {
            _slotsAreLoad = load;
            _pause.SetActive(false);
            RebuildSlotList();
            var title = _slots.transform.Find("TVFrame/Title")?.GetComponent<Text>();
            if (title != null) title.text = load ? "ЗАГРУЗИТЬ" : "СОХРАНИТЬ";
            _slots.SetActive(true);
        }

        void RebuildSlotList()
        {
            var frame = _slots.transform.Find("TVFrame");
            var list = frame.Find("List");
            if (list == null) return;
            for (var i = list.childCount - 1; i >= 0; i--)
                Destroy(list.GetChild(i).gameObject);

            var metas = SaveService.Instance.ListSlots();
            // Show manuals first then autos, stable order by kind/index for UX
            var ordered = new List<SaveMeta>();
            for (var i = 0; i < SaveService.ManualSlotCount; i++)
                ordered.Add(FindMeta(metas, SaveKind.Manual, i));
            for (var i = 0; i < SaveService.AutoSlotCount; i++)
                ordered.Add(FindMeta(metas, SaveKind.Auto, i));

            float y = 260;
            foreach (var meta in ordered)
            {
                var captured = meta;
                var label = FormatMeta(captured);
                var empty = captured.unixTimestamp <= 0;
                if (_slotsAreLoad && empty) continue;

                UiFactory.Button(list, captured.slotId, label, new Vector2(0, y), new Vector2(700, 56), () =>
                {
                    if (_slotsAreLoad)
                        ConfirmLoad(captured);
                    else
                        ConfirmSave(captured);
                });
                y -= 64;
            }

            if (_slotsAreLoad && ordered.TrueForAll(m => m.unixTimestamp <= 0))
            {
                UiFactory.Label(list, "Empty", "Нет сохранений", 28, TextAnchor.MiddleCenter, Dim);
            }
        }

        SaveMeta FindMeta(IReadOnlyList<SaveMeta> metas, SaveKind kind, int index)
        {
            foreach (var m in metas)
                if (m.kind == kind && m.index == index) return m;
            return new SaveMeta { kind = kind, index = index, unixTimestamp = 0, summaryLabel = "Пустой слот" };
        }

        string FormatMeta(SaveMeta m)
        {
            if (m.unixTimestamp <= 0)
                return $"{(m.kind == SaveKind.Manual ? "Ручное" : "Авто")} {m.index + 1} · Пустой слот";

            var dt = DateTimeOffset.FromUnixTimeSeconds(m.unixTimestamp).ToLocalTime().ToString("g");
            var kind = m.kind == SaveKind.Manual ? "Ручное" : "Авто";
            var loc = string.IsNullOrEmpty(m.locationId) ? "-" : m.locationId;
            return $"{kind} {m.index + 1} · {dt} · {loc} · {Mathf.RoundToInt(m.playtimeSec)}с";
        }

        void ConfirmSave(SaveMeta slot)
        {
            if (SaveService.Instance == null) return;
            if (!SaveService.Instance.CanManualSave)
            {
                ShowModal(SaveService.Instance.BlockHint, null);
                return;
            }

            void DoSave()
            {
                var payload = GameSession.Instance.BuildPayload();
                if (!SaveService.Instance.TrySaveManual(slot.index, payload, out var err))
                    ShowModal(err ?? "Не удалось сохранить.", null);
                else
                {
                    _slots.SetActive(false);
                    _pause.SetActive(true);
                    ShowModal("Сохранено.", null, okOnly: true);
                }
            }

            if (slot.unixTimestamp > 0)
                ShowModal("Перезаписать слот?", DoSave);
            else
                DoSave();
        }

        void ConfirmLoad(SaveMeta slot)
        {
            ShowModal("Загрузить это сохранение? Текущий прогресс сессии будет потерян.", () =>
            {
                if (!SaveService.Instance.TryLoad(slot.kind, slot.index, out var payload) || payload == null)
                {
                    ShowModal("Не удалось загрузить.", null);
                    return;
                }

                _slots.SetActive(false);
                SetPause(false);
                GameSession.Instance.ContinueFromPayload(payload);
            });
        }

        void ShowModal(string text, Action onConfirm, bool cancelOnly = false, bool okOnly = false)
        {
            _modalText.text = text;
            _modalConfirm = onConfirm;
            _modal.SetActive(true);
            var ok = _modal.transform.Find("TVFrame/Ok")?.gameObject;
            var cancel = _modal.transform.Find("TVFrame/Cancel")?.gameObject;
            if (okOnly)
            {
                if (ok != null) ok.SetActive(true);
                if (cancel != null) cancel.SetActive(false);
                _modalConfirm = () => { };
            }
            else if (onConfirm == null)
            {
                if (ok != null) ok.SetActive(true);
                if (cancel != null) cancel.SetActive(false);
            }
            else
            {
                if (ok != null) ok.SetActive(true);
                if (cancel != null) cancel.SetActive(true);
            }
        }

        static string PresetName(GraphicsPreset p)
        {
            switch (p)
            {
                case GraphicsPreset.Low: return "Низкая";
                case GraphicsPreset.High: return "Высокая";
                default: return "Средняя";
            }
        }
    }
}
