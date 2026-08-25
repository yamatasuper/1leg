using System;
using System.Collections;
using NinetyMinutes.Art;
using NinetyMinutes.Save;
using NinetyMinutes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NinetyMinutes.Match
{
    /// <summary>
    /// Comic match-beat presentation: page flip + 2 templates. No skip.
    /// </summary>
    public sealed class MatchPresentation : MonoBehaviour
    {
        public static MatchPresentation Instance { get; private set; }

        public bool IsPlaying { get; private set; }

        Canvas _canvas;
        GameObject _root;
        Image _veil;
        Image _page;
        Image _panelA;
        Image _panelB;
        Image _panelC;
        Text _caption;
        Text _flipLabel;
        string _lastTemplateId;

        static readonly Color PageMatch = new Color(0.12f, 0.16f, 0.14f, 1f);
        static readonly Color PagePast = new Color(0.22f, 0.18f, 0.12f, 1f);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            Build();
            _root.SetActive(false);
        }

        void Build()
        {
            _canvas = UiFactory.CreateCanvas("MatchPresentationCanvas", 300);
            DontDestroyOnLoad(_canvas.gameObject);
            _root = UiFactory.Panel(_canvas.transform, "Root", new Color(0, 0, 0, 0.96f)).gameObject;

            _veil = _root.GetComponent<Image>();
            _page = UiFactory.Box(_root.transform, "Page", Vector2.zero, new Vector2(1400, 820), PageMatch)
                .GetComponent<Image>();

            _panelA = MakePanel("P1", new Vector2(-420, 80), new Vector2(360, 420));
            _panelB = MakePanel("P2", new Vector2(0, 80), new Vector2(360, 420));
            _panelC = MakePanel("P3", new Vector2(420, 80), new Vector2(360, 420));

            var capBox = UiFactory.Box(_page.transform, "CaptionBox", new Vector2(0, -320), new Vector2(1100, 120),
                new Color(0.05f, 0.06f, 0.07f, 0.9f));
            _caption = UiFactory.Label(capBox, "Caption", "", 28, TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.88f));

            _flipLabel = UiFactory.Title(_root.transform, "FlipLabel", "", 36, new Vector2(0, -40),
                new Color(0.95f, 0.92f, 0.4f));
        }

        Image MakePanel(string name, Vector2 pos, Vector2 size)
        {
            var rt = UiFactory.Box(_page.transform, name, pos, size, new Color(0.25f, 0.28f, 0.3f));
            var outline = rt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.7f, 0.75f, 0.55f, 0.7f);
            outline.effectDistance = new Vector2(2, -2);
            UiFactory.Label(rt, "Mark", name, 22, TextAnchor.UpperLeft, new Color(1, 1, 1, 0.35f));
            return rt.GetComponent<Image>();
        }

        public void Play(MatchBeatRequest request, Action onComplete)
        {
            if (request == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (IsPlaying)
            {
                Debug.LogWarning("[90 минут] Match beat already playing.");
                return;
            }

            StartCoroutine(PlayRoutine(request, onComplete));
        }

        public void PlayIntro(Action onComplete)
        {
            if (IsPlaying)
            {
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(PlayIntroRoutine(onComplete));
        }

        public void PlayFinalWhistle(Action onComplete)
        {
            if (IsPlaying)
            {
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(PlayWhistleRoutine(onComplete));
        }

        IEnumerator PlayIntroRoutine(Action onComplete)
        {
            IsPlaying = true;
            if (SaveService.Instance != null)
                SaveService.Instance.SetBlockReason(SaveBlockReason.MatchPresentation);

            _root.SetActive(true);
            yield return FlipIn();
            MatchFrameUI.Instance?.Show();
            MatchFrameUI.Instance?.Refresh();

            SetPanelArt(_panelA, ArtCatalog.MatchAction);
            SetPanelArt(_panelB, ArtCatalog.LocationStreet);
            SetPanelArt(_panelC, ArtCatalog.MatchAction);
            _caption.text = "Западная трибуна закрыта. Река за воротами не спрашивает.";
            yield return PanelHold(3.5f);
            _caption.text = "Иногда матч начинается раньше свистка.";
            yield return PanelHold(3.5f);
            _caption.text = "И иногда матч начинается раньше свистка — там, где никто не считает голы.";
            yield return PanelHold(3f);

            yield return FlipOut();
            MatchFrameUI.Instance?.Hide();
            _root.SetActive(false);

            if (SaveService.Instance != null)
                SaveService.Instance.SetBlockReason(SaveBlockReason.None);
            IsPlaying = false;
            onComplete?.Invoke();
        }

        IEnumerator PlayWhistleRoutine(Action onComplete)
        {
            IsPlaying = true;
            if (SaveService.Instance != null)
                SaveService.Instance.SetBlockReason(SaveBlockReason.MatchPresentation);

            _root.SetActive(true);
            yield return FlipIn();
            MatchFrameUI.Instance?.Show();
            MatchFrameUI.Instance?.Refresh();
            ChoiceScoreBridge.Instance?.SetMinute(90);

            SetPanelArt(_panelA, ArtCatalog.MatchAction);
            SetPanelArt(_panelB, ArtCatalog.MatchGoal);
            SetPanelArt(_panelC, ArtCatalog.PortraitBardin);
            _caption.text = MatchCaptions.FinalWhistle;
            yield return PanelHold(5f);

            yield return FlipOut();
            MatchFrameUI.Instance?.Hide();
            _root.SetActive(false);

            if (SaveService.Instance != null)
                SaveService.Instance.SetBlockReason(SaveBlockReason.None);
            IsPlaying = false;
            onComplete?.Invoke();
        }

        IEnumerator PlayRoutine(MatchBeatRequest request, Action onComplete)
        {
            IsPlaying = true;
            if (SaveService.Instance != null)
                SaveService.Instance.SetBlockReason(SaveBlockReason.MatchPresentation);

            _root.SetActive(true);
            MatchFrameUI.Instance?.Hide();

            var primary = ChoiceScoreBridge.Instance != null
                ? ChoiceScoreBridge.Instance.PrimaryOutcome(request.Pack)
                : OutcomeType.Miss;
            var templateId = PickTemplate(primary);
            var isGoal = primary == OutcomeType.GoalFor || primary == OutcomeType.GoalAgainst;
            var totalSec = isGoal ? 18f : 12f;

            // Flip in
            yield return FlipIn();

            // Apply score mid-beat (presentation sync)
            ApplyPanels(templateId, primary);
            _caption.text = CaptionFor(primary, 0);
            MatchFrameUI.Instance?.Show();
            MatchFrameUI.Instance?.Refresh();

            yield return PanelHold(totalSec * 0.28f);

            // Scoreboard moment + apply deltas
            if (ChoiceScoreBridge.Instance != null && request.Pack != null)
            {
                ChoiceScoreBridge.Instance.ApplyPackToScore(request.Pack);
                ChoiceScoreBridge.Instance.SetMinute(request.MinuteAfter);
            }

            MatchFrameUI.Instance?.PulseScore();
            _caption.text = CaptionFor(primary, 1);
            TintPanelsForOutcome(primary);
            yield return PanelHold(totalSec * 0.36f);

            _caption.text = CaptionFor(primary, 2);
            yield return PanelHold(totalSec * 0.22f);

            // Flip out to past
            yield return FlipOut();

            MatchFrameUI.Instance?.Hide();
            _root.SetActive(false);

            if (SaveService.Instance != null)
                SaveService.Instance.SetBlockReason(SaveBlockReason.None);

            IsPlaying = false;
            onComplete?.Invoke();
        }

        string PickTemplate(OutcomeType primary)
        {
            // Sprint 2: two templates — attack / setback
            var attack = primary == OutcomeType.GoalFor || primary == OutcomeType.DribbleWin;
            var id = attack ? "tpl_attack" : "tpl_setback";
            _lastTemplateId = id;
            return id;
        }

        void ApplyPanels(string templateId, OutcomeType primary)
        {
            var action = ArtCatalog.MatchAction;
            var goal = ArtCatalog.MatchGoal;
            SetPanelArt(_panelA, action);
            SetPanelArt(_panelB, action);
            SetPanelArt(_panelC, primary == OutcomeType.GoalFor ? goal : action);
            _page.color = PageMatch;
        }

        static void SetPanelArt(Image img, Sprite sprite)
        {
            if (img == null) return;
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = true;
                img.type = Image.Type.Simple;
            }
            else
            {
                img.sprite = null;
                img.color = new Color(0.25f, 0.28f, 0.3f);
            }
        }

        void TintPanelsForOutcome(OutcomeType primary)
        {
            // Keep photo panels; light punch via color multiply
            if (_panelC == null) return;
            if (primary == OutcomeType.GoalFor)
                _panelC.color = new Color(1f, 1.05f, 0.9f);
            else if (primary == OutcomeType.GoalAgainst)
                _panelC.color = new Color(1f, 0.75f, 0.75f);
            else
                _panelC.color = Color.white;
        }

        static string CaptionFor(OutcomeType type, int stage)
        {
            switch (type)
            {
                case OutcomeType.GoalFor:
                    return stage == 0 ? "Удар назревает…"
                        : stage == 1 ? MatchCaptions.GoalFor01
                        : MatchCaptions.GoalFor02;
                case OutcomeType.GoalAgainst:
                    return stage == 0 ? "Брешь в обороне…"
                        : stage == 1 ? MatchCaptions.GoalAgainst01
                        : MatchCaptions.GoalAgainst02;
                case OutcomeType.Miss:
                    return stage <= 1 ? MatchCaptions.Miss01 : MatchCaptions.Miss02;
                case OutcomeType.BallOut:
                    return MatchCaptions.BallOut01;
                case OutcomeType.DribbleWin:
                    return MatchCaptions.DribbleWin01;
                case OutcomeType.DribbleLose:
                    return MatchCaptions.DribbleLose01;
                default:
                    return "Тишина на трибунах.";
            }
        }

        IEnumerator FlipIn()
        {
            _flipLabel.text = MatchCaptions.FlipToMatch;
            _flipLabel.gameObject.SetActive(true);
            _page.color = PageMatch;
            var t = 0f;
            var dur = 0.6f;
            var rt = _page.rectTransform;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.Clamp01(t / dur);
                rt.localEulerAngles = new Vector3(0, Mathf.Lerp(-75f, 0f, u), 0);
                rt.localScale = new Vector3(Mathf.Lerp(0.2f, 1f, u), 1f, 1f);
                yield return null;
            }

            rt.localEulerAngles = Vector3.zero;
            rt.localScale = Vector3.one;
            _flipLabel.gameObject.SetActive(false);
        }

        IEnumerator FlipOut()
        {
            _flipLabel.text = MatchCaptions.FlipToPast;
            _flipLabel.gameObject.SetActive(true);
            var t = 0f;
            var dur = 0.6f;
            var rt = _page.rectTransform;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.Clamp01(t / dur);
                _page.color = Color.Lerp(PageMatch, PagePast, u);
                rt.localEulerAngles = new Vector3(0, Mathf.Lerp(0f, 75f, u), 0);
                rt.localScale = new Vector3(Mathf.Lerp(1f, 0.2f, u), 1f, 1f);
                yield return null;
            }

            rt.localEulerAngles = Vector3.zero;
            rt.localScale = Vector3.one;
            _flipLabel.gameObject.SetActive(false);
        }

        IEnumerator PanelHold(float sec)
        {
            var t = 0f;
            while (t < sec)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }

    public static class MatchCaptions
    {
        public const string FlipToMatch = "Страница матча";
        public const string FlipToPast = "Страница памяти";
        public const string GoalFor01 = "Есть! Смотрите на этот удар!";
        public const string GoalFor02 = "Гол — как выдох, который долго держали.";
        public const string GoalAgainst01 = "И… вот это уже опасно.";
        public const string GoalAgainst02 = "Мяч в сетке. В груди — тоже дыра.";
        public const string Miss01 = "Промах. Стадион замер.";
        public const string Miss02 = "Мимо. Иногда честность не получает награды.";
        public const string BallOut01 = "Мяч уходит за линию. Передышка.";
        public const string DribbleWin01 = "Обводка! Он проходит!";
        public const string DribbleLose01 = "Отбирают мяч. Жёстко.";
        public const string FinalWhistle = "Финальный свисток. Счёт записан. История — ещё нет.";
    }
}
