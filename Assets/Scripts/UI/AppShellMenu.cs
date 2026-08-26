using System;
using System.Collections.Generic;
using NinetyMinutes.Art;
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

        static readonly Color Cream = new Color(0.96f, 0.93f, 0.86f, 1f);
        static readonly Color Mustard = new Color(0.78f, 0.58f, 0.22f, 1f);
        static readonly Color Teal = new Color(0.62f, 0.7f, 0.68f, 1f);
        static readonly Color Dim = new Color(0.72f, 0.7f, 0.62f, 1f);
        static readonly Color Wash = new Color(0.07f, 0.06f, 0.05f, 0.72f);

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
            _main = UiFactory.Panel(_canvas.transform, "MainMenu", new Color(0.08f, 0.08f, 0.07f, 1f)).gameObject;

            var art = UiFactory.FullImage(_main.transform, "Art", ArtCatalog.MenuKey, new Color(0.18f, 0.16f, 0.12f));
            art.gameObject.AddComponent<MenuKenBurns>();

            var washGo = new GameObject("Wash", typeof(RectTransform), typeof(Image));
            washGo.transform.SetParent(_main.transform, false);
            var washRt = washGo.GetComponent<RectTransform>();
            washRt.anchorMin = new Vector2(0f, 0f);
            washRt.anchorMax = new Vector2(0.46f, 1f);
            washRt.offsetMin = Vector2.zero;
            washRt.offsetMax = Vector2.zero;
            washGo.GetComponent<Image>().color = Wash;
            washGo.GetComponent<Image>().raycastTarget = false;

            var portrait = ArtCatalog.PortraitBardin;
            if (portrait != null)
            {
                var p = UiFactory.Box(_main.transform, "Portrait", new Vector2(620f, -40f), new Vector2(520f, 720f), Color.clear);
                p.anchorMin = p.anchorMax = new Vector2(1f, 0.5f);
                p.pivot = new Vector2(1f, 0.5f);
                p.anchoredPosition = new Vector2(-36f, -20f);
                var img = p.GetComponent<Image>();
                img.sprite = portrait;
                img.color = new Color(1f, 1f, 1f, 0.92f);
                img.preserveAspect = true;
                img.raycastTarget = false;
            }

            var frame = UiFactory.LeftColumn(_main.transform, "TVFrame", 640f, 72f);
            UiFactory.Headline(frame, "OnAir", "ПОСЛЕДНИЙ МАТЧ", 22, new Vector2(0, -72), new Vector2(600, 36), Mustard, TextAnchor.UpperLeft);
            UiFactory.Headline(frame, "Title", "90 МИНУТ", 78, new Vector2(0, -108), new Vector2(620, 96), Cream, TextAnchor.UpperLeft);
            UiFactory.Hairline(frame, "Rule", new Vector2(0, 0), new Vector2(220, 3), Mustard)
                .rectTransform.anchorMin = new Vector2(0f, 1f);
            var rule = frame.Find("Rule") as RectTransform;
            if (rule != null)
            {
                rule.anchorMin = rule.anchorMax = new Vector2(0f, 1f);
                rule.pivot = new Vector2(0f, 1f);
                rule.anchoredPosition = new Vector2(0f, -214f);
            }

            UiFactory.Headline(frame, "Sub", "Алексей Бардин · «Торпедо» · Прибрежье", 22, new Vector2(0, -232), new Vector2(600, 40), Teal, TextAnchor.UpperLeft);
            UiFactory.Headline(frame, "Line", "Река за воротами не спрашивает, сколько тебе лет.", 20, new Vector2(0, -272), new Vector2(560, 48), Dim, TextAnchor.UpperLeft);

            float y = -20f;
            UiFactory.GhostButton(frame, "BtnNew", "Новая игра", new Vector2(0, y), new Vector2(420, 58), OnNewGame);
            y -= 70;
            var cont = UiFactory.GhostButton(frame, "BtnContinue", "Продолжить", new Vector2(0, y), new Vector2(420, 58), OnContinue);
            cont.name = "BtnContinue";
            y -= 70;
            UiFactory.GhostButton(frame, "BtnSettings", "Настройки", new Vector2(0, y), new Vector2(420, 58), () => ShowSettings(fromPause: false));
            y -= 70;
            UiFactory.GhostButton(frame, "BtnQuit", "Выход", new Vector2(0, y), new Vector2(420, 58), OnQuit);

            _status = UiFactory.Footer(frame, "Status", "", 18, Dim);
            _status.alignment = TextAnchor.MiddleLeft;
            _status.rectTransform.anchoredPosition = new Vector2(8f, 36f);
        }

        void BuildPause()
        {
            _pause = UiFactory.Panel(_canvas.transform, "PauseMenu", new Color(0.04f, 0.03f, 0.02f, 0.72f)).gameObject;
            var frame = UiFactory.PaperCard(_pause.transform, "TVFrame", Vector2.zero, new Vector2(640, 680));
            UiFactory.Headline(frame, "Title", "ПАУЗА", 42, new Vector2(0, -28), new Vector2(560, 56), Cream, TextAnchor.UpperCenter);

            float y = 140;
            UiFactory.GhostButton(frame, "Resume", "Продолжить", new Vector2(0, y), new Vector2(400, 54), () => SetPause(false));
            y -= 66;
            UiFactory.GhostButton(frame, "Save", "Сохранить", new Vector2(0, y), new Vector2(400, 54), () => OpenSlots(load: false));
            y -= 66;
            UiFactory.GhostButton(frame, "Load", "Загрузить", new Vector2(0, y), new Vector2(400, 54), () => OpenSlots(load: true));
            y -= 66;
            UiFactory.GhostButton(frame, "Settings", "Настройки", new Vector2(0, y), new Vector2(400, 54), () => ShowSettings(fromPause: true));
            y -= 66;
            UiFactory.GhostButton(frame, "ToTitle", "В главное меню", new Vector2(0, y), new Vector2(400, 54), ConfirmToTitle);
            y -= 66;
            UiFactory.GhostButton(frame, "Quit", "Выход", new Vector2(0, y), new Vector2(400, 54), ConfirmQuit);
            _pause.SetActive(false);
        }

        void BuildSettings()
        {
            _settings = UiFactory.Panel(_canvas.transform, "Settings", new Color(0.04f, 0.03f, 0.02f, 0.78f)).gameObject;
            var frame = UiFactory.PaperCard(_settings.transform, "TVFrame", Vector2.zero, new Vector2(720, 820));
            UiFactory.Headline(frame, "Title", "НАСТРОЙКИ", 36, new Vector2(0, -24), new Vector2(640, 48), Cream, TextAnchor.UpperCenter);

            float y = 200;
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

            UiFactory.GhostButton(frame, "Back", "Назад", new Vector2(0, -340), new Vector2(360, 54), HideSettings);
            _settings.SetActive(false);
        }

        void AddCycleButton(Transform parent, string name, Func<string> cycle, ref float y)
        {
            var btn = UiFactory.GhostButton(parent, name, RefreshSettingLabel(name), new Vector2(0, y), new Vector2(520, 50), null);
            btn.onClick.AddListener(() =>
            {
                var t = btn.GetComponentInChildren<Text>();
                if (t != null) t.text = cycle();
            });
            y -= 50;
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
            _slots = UiFactory.Panel(_canvas.transform, "Slots", new Color(0.04f, 0.03f, 0.02f, 0.82f)).gameObject;
            var frame = UiFactory.PaperCard(_slots.transform, "TVFrame", Vector2.zero, new Vector2(860, 820));
            UiFactory.Headline(frame, "Title", "СОХРАНЕНИЯ", 36, new Vector2(0, -24), new Vector2(760, 48), Cream, TextAnchor.UpperCenter);
            // Content rebuilt each open
            var list = new GameObject("List", typeof(RectTransform));
            list.transform.SetParent(frame, false);
            var rt = list.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, -20);
            rt.sizeDelta = new Vector2(760, 620);
            UiFactory.GhostButton(frame, "Back", "Назад", new Vector2(0, -360), new Vector2(300, 52), () =>
            {
                _slots.SetActive(false);
                if (_pauseOpen) _pause.SetActive(true);
            });
            _slots.SetActive(false);
        }

        void BuildModal()
        {
            _modal = UiFactory.Panel(_canvas.transform, "Modal", new Color(0.04f, 0.03f, 0.02f, 0.82f)).gameObject;
            var frame = UiFactory.PaperCard(_modal.transform, "TVFrame", Vector2.zero, new Vector2(680, 300));
            _modalText = UiFactory.Label(frame, "Text", "", 24, TextAnchor.MiddleCenter, Cream);
            _modalText.rectTransform.offsetMin = new Vector2(30, 90);
            _modalText.rectTransform.offsetMax = new Vector2(-30, -70);
            UiFactory.GhostButton(frame, "Ok", "Да", new Vector2(-120, -96), new Vector2(200, 50), () =>
            {
                _modal.SetActive(false);
                _modalConfirm?.Invoke();
                _modalConfirm = null;
            });
            UiFactory.GhostButton(frame, "Cancel", "Нет", new Vector2(120, -96), new Vector2(200, 50), () =>
            {
                _modal.SetActive(false);
                _modalConfirm = null;
            });
            _modal.SetActive(false);
        }

        void BuildGameplayStub()
        {
            _gameplayStub = UiFactory.Panel(_canvas.transform, "GameplayStub", new Color(0.07f, 0.06f, 0.05f, 1f)).gameObject;
            _gameplayStub.transform.SetAsFirstSibling();
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
                    ? "Есть сохранения · можно продолжить"
                    : "Нет сохранений · начни с раздевалки";
            }
        }

        void RefreshContinueButton()
        {
            var btn = _main.transform.Find("TVFrame/BtnContinue")?.GetComponent<Button>();
            if (btn == null) return;
            var has = SaveService.Instance != null && SaveService.Instance.HasAnySave();
            btn.interactable = has;
            var t = btn.GetComponentInChildren<Text>();
            if (t != null) t.color = has ? Cream : Dim;
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

                UiFactory.GhostButton(list, captured.slotId, label, new Vector2(0, y), new Vector2(700, 56), () =>
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
