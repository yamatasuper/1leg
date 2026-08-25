using System.Collections;
using NinetyMinutes.Core;
using NinetyMinutes.Dialogue;
using NinetyMinutes.Match;
using NinetyMinutes.Save;
using NinetyMinutes.Stats;
using NinetyMinutes.UI;
using NinetyMinutes.World;
using UnityEngine;

namespace NinetyMinutes.Narrative
{
    /// <summary>
    /// Slice spine: intro → training → street dialogues (no mid-match) → one match → ending.
    /// </summary>
    public sealed class SliceDirector : MonoBehaviour
    {
        public static SliceDirector Instance { get; private set; }

        public static readonly string[] StreetDialogueFlags =
        {
            "street_glock_done",
            "street_sokol_done",
            "street_self_done"
        };

        public SlicePhase Phase { get; private set; } = SlicePhase.None;
        DialogueGraph _lastGraph;
        bool _busy;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        void Start() => BindDialogue();

        void BindDialogue()
        {
            if (DialogueRunner.Instance == null) return;
            DialogueRunner.Instance.Opened -= OnDialogueOpened;
            DialogueRunner.Instance.Closed -= OnDialogueClosed;
            DialogueRunner.Instance.Opened += OnDialogueOpened;
            DialogueRunner.Instance.Closed += OnDialogueClosed;
        }

        public void ResetForNewRun()
        {
            Phase = SlicePhase.None;
            _busy = false;
            _lastGraph = null;
        }

        public void BeginNewRun()
        {
            BindDialogue();
            ResetForNewRun();
            StartCoroutine(RunIntroThenTraining());
        }

        public void ResumeFromSave(SlicePhase phase)
        {
            BindDialogue();
            // Migrate old sprint-3 phase names
            if ((int)phase >= 4 && (int)phase <= 8 && phase != SlicePhase.Match)
                phase = SlicePhase.StreetLife;

            Phase = phase == SlicePhase.None ? SlicePhase.Training : phase;
            _busy = false;

            if (Phase == SlicePhase.Intro)
            {
                StartCoroutine(RunIntroThenTraining());
                return;
            }

            if (Phase >= SlicePhase.EndingCard)
            {
                Phase = SlicePhase.Finished;
                WorldController.Instance?.RefreshSpine(Phase);
                return;
            }

            if (Phase == SlicePhase.Match)
                Phase = SlicePhase.StreetLife;

            WorldController.Instance?.RefreshSpine(Phase);
            ApplyActMeta();

            if (Phase == SlicePhase.StreetLife && StreetSegmentComplete())
                StartCoroutine(RunMatchThenEnding());
        }

        IEnumerator RunIntroThenTraining()
        {
            _busy = true;
            Phase = SlicePhase.Intro;
            ApplyActMeta();

            WorldController.Instance?.StopWorld();
            var player = FindObjectOfType<PlayerController>();
            if (player != null) player.InputLocked = true;

            if (ChoiceScoreBridge.Instance != null)
            {
                ChoiceScoreBridge.Instance.ResetForNewRun();
                ChoiceScoreBridge.Instance.SetMinute(1);
            }

            var introDone = false;
            MatchPresentation.Instance?.PlayIntro(() => introDone = true);
            while (!introDone) yield return null;

            var cardDone = false;
            void OnClosed()
            {
                if (_lastGraph != null && _lastGraph.Id == "dlg_intro_flashback")
                {
                    DialogueRunner.Instance.Closed -= OnClosed;
                    cardDone = true;
                }
            }

            DialogueRunner.Instance.Closed += OnClosed;
            DialogueRunner.Instance.StartDialogue(SliceDialogues.IntroFlashback());
            while (!cardDone) yield return null;

            Phase = SlicePhase.Training;
            ApplyActMeta();
            WorldController.Instance?.StartOrResumeWorld("loc_locker", new Vector2(0f, -1.5f));
            WorldController.Instance?.RefreshSpine(Phase);
            _busy = false;
            Autosave("after_intro");
        }

        void OnDialogueOpened()
        {
            _lastGraph = DialogueRunner.Instance?.ActiveGraph;
        }

        void OnDialogueClosed()
        {
            if (_busy) return;
            var graph = _lastGraph;
            _lastGraph = null;
            if (graph == null) return;
            StartCoroutine(HandleDialogueClosed(graph.Id));
        }

        IEnumerator HandleDialogueClosed(string graphId)
        {
            yield return null;
            if (_busy) yield break;

            switch (graphId)
            {
                case "dlg_train_coach":
                    SliceDialogues.Flags.Add("training_done");
                    SoftStatsService.Instance?.Apply("morale", 1);
                    SoftStatsService.Instance?.Apply("energy", 1);
                    EnterStreetLife();
                    break;

                case "dlg_train_skip":
                    if (SliceDialogues.Flags.Contains("training_skipped"))
                    {
                        SliceDialogues.Flags.Add("training_done");
                        EnterStreetLife();
                    }
                    break;

                case "dlg_seg1_glock":
                    SliceDialogues.Flags.Add("street_glock_done");
                    WorldController.Instance?.RefreshSpine(Phase);
                    Autosave("street_glock");
                    if (StreetSegmentComplete())
                        yield return StartCoroutine(RunMatchThenEnding());
                    break;

                case "dlg_seg2_sokol":
                    SliceDialogues.Flags.Add("street_sokol_done");
                    ChoiceScoreBridge.Instance?.MarkTwist();
                    WorldController.Instance?.RefreshSpine(Phase);
                    Autosave("street_sokol");
                    if (StreetSegmentComplete())
                        yield return StartCoroutine(RunMatchThenEnding());
                    break;

                case "dlg_seg3_self":
                    SliceDialogues.Flags.Add("street_self_done");
                    WorldController.Instance?.RefreshSpine(Phase);
                    Autosave("street_self");
                    if (StreetSegmentComplete())
                        yield return StartCoroutine(RunMatchThenEnding());
                    break;
            }
        }

        void EnterStreetLife()
        {
            Phase = SlicePhase.StreetLife;
            ApplyActMeta();
            ChoiceScoreBridge.Instance?.BeginSegment("slice_street");
            WorldController.Instance?.TravelTo("loc_street", new Vector2(0f, -1.5f));
            WorldController.Instance?.RefreshSpine(Phase);
            Autosave("enter_street");
        }

        public static bool StreetSegmentComplete()
        {
            foreach (var f in StreetDialogueFlags)
            {
                if (!SliceDialogues.Flags.Contains(f))
                    return false;
            }

            return true;
        }

        IEnumerator RunMatchThenEnding()
        {
            if (_busy) yield break;
            _busy = true;
            Phase = SlicePhase.Match;
            ApplyActMeta();
            WorldController.Instance?.RefreshSpine(Phase);

            var beatDone = false;
            MatchBeatDirector.Instance?.PlayBeat("slice_street", 90, forceIrony: false, () => beatDone = true);
            while (!beatDone) yield return null;

            EndingsService.Instance?.ResolveAndLock();

            var whistleDone = false;
            MatchPresentation.Instance?.PlayFinalWhistle(() => whistleDone = true);
            while (!whistleDone) yield return null;

            yield return StartCoroutine(ShowEndingFlow());
        }

        IEnumerator ShowEndingFlow()
        {
            Phase = SlicePhase.EndingCard;
            ApplyActMeta();
            WorldController.Instance?.SuspendForMatch(true);

            var route = EndingsService.Instance != null
                ? EndingsService.Instance.LockedRoute
                : EndingRoute.Mid;
            if (route == EndingRoute.None)
                route = EndingsService.Instance?.ResolveAndLock() ?? EndingRoute.Mid;

            var cardDone = false;
            EndingCardUI.Instance?.ShowEnding(route, () => cardDone = true);
            while (!cardDone) yield return null;

            Phase = SlicePhase.Credits;
            var creditsDone = false;
            EndingCardUI.Instance?.ShowCredits(() => creditsDone = true);
            while (!creditsDone) yield return null;

            Phase = SlicePhase.Finished;
            _busy = false;
            Autosave("run_finished");
            GameSession.Instance?.EnterMainMenu();
        }

        void ApplyActMeta()
        {
            if (GameSession.Instance == null) return;
            GameSession.Instance.ActId = Phase.ToString();
            switch (Phase)
            {
                case SlicePhase.Intro:
                case SlicePhase.Match:
                    GameSession.Instance.TimeMode = "match";
                    break;
                case SlicePhase.EndingCard:
                case SlicePhase.Credits:
                    GameSession.Instance.TimeMode = "post";
                    break;
                default:
                    GameSession.Instance.TimeMode = "past";
                    break;
            }
        }

        void Autosave(string anchor)
        {
            if (GameSession.Instance != null && SaveService.Instance != null)
                SaveService.Instance.TrySaveAuto(anchor, GameSession.Instance.BuildPayload(), out _);
        }
    }
}
