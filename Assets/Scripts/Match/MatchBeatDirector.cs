using System;
using System.Collections;
using NinetyMinutes.Core;
using NinetyMinutes.Dialogue;
using NinetyMinutes.Save;
using NinetyMinutes.World;
using UnityEngine;

namespace NinetyMinutes.Match
{
    /// <summary>
    /// Plays a match beat on demand (after a whole past segment of dialogues).
    /// </summary>
    public sealed class MatchBeatDirector : MonoBehaviour
    {
        public static MatchBeatDirector Instance { get; private set; }

        public bool IsPlaying { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void PlayBeat(string segmentId, int minuteAfter, bool forceIrony, Action onComplete)
        {
            if (IsPlaying)
            {
                Debug.LogWarning("[90 минут] Match beat already playing.");
                return;
            }

            StartCoroutine(RunBeat(segmentId, minuteAfter, forceIrony, onComplete));
        }

        IEnumerator RunBeat(string segmentId, int minuteAfter, bool forceIrony, Action onComplete)
        {
            IsPlaying = true;
            var bridge = ChoiceScoreBridge.Instance;
            var presentation = MatchPresentation.Instance;
            if (bridge == null || presentation == null)
            {
                IsPlaying = false;
                onComplete?.Invoke();
                yield break;
            }

            var player = FindObjectOfType<PlayerController>();
            if (player != null) player.InputLocked = true;
            WorldController.Instance?.SuspendForMatch(true);

            if (GameSession.Instance != null)
            {
                GameSession.Instance.TimeMode = "match";
                GameSession.Instance.ActId = "match";
            }

            if (forceIrony)
                bridge.MarkTwist();

            var pack = bridge.BuildOutcomePack(segmentId);
            var request = new MatchBeatRequest
            {
                BeatId = $"beat_{segmentId}",
                SegmentId = segmentId,
                Pack = pack,
                MinuteAfter = minuteAfter > 0 ? minuteAfter : 90,
                ScoreBefore = new MatchScore
                {
                    GoalsFor = bridge.Score.GoalsFor,
                    GoalsAgainst = bridge.Score.GoalsAgainst
                },
                OpponentName = bridge.OpponentName
            };

            var done = false;
            presentation.Play(request, () => done = true);
            while (!done) yield return null;

            if (GameSession.Instance != null)
                GameSession.Instance.TimeMode = "past";

            WorldController.Instance?.SuspendForMatch(false);
            if (player != null) player.InputLocked = false;

            if (GameSession.Instance != null && SaveService.Instance != null)
                SaveService.Instance.TrySaveAuto("after_match_beat", GameSession.Instance.BuildPayload(), out _);

            Debug.Log(
                $"[90 минут] Match beat done · {bridge.PrimaryOutcome(pack)} · {bridge.Score.GoalsFor}:{bridge.Score.GoalsAgainst} · {bridge.MatchMinute}'");

            IsPlaying = false;
            onComplete?.Invoke();
        }
    }
}
