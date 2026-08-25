using System;
using System.Collections.Generic;
using NinetyMinutes.Stats;
using UnityEngine;

namespace NinetyMinutes.Match
{
    /// <summary>
    /// Hidden choice → score bridge. Player never sees pulse/tags.
    /// </summary>
    public sealed class ChoiceScoreBridge : MonoBehaviour
    {
        public static ChoiceScoreBridge Instance { get; private set; }

        public float FormPulse { get; private set; }
        public MatchScore Score { get; } = new MatchScore();
        public int MatchMinute { get; private set; } = 1;
        public int GoalsEventsThisHalf { get; private set; }
        public string OpponentName { get; set; } = "Прибой";
        public bool SoftStatsBiasEnabled { get; set; } = true;

        // Balance (slice-tuned thresholds so 2× push_up can score)
        public float PushUpWeight = 1f;
        public float PushDownWeight = 1f;
        public float GoalForThreshold = 2f;
        public float GoalAgainstThreshold = -2f;
        public float GoalForConsume = 2f;
        public float GoalAgainstRecover = 2f;
        public int MaxGoalsEventsPerHalf = 3;
        public int MaxGoalsForMatch = 5;
        public int MaxGoalsAgainstMatch = 5;

        readonly List<string> _segmentTags = new List<string>();
        bool _segmentHasTwist;

        public event Action ScoreChanged;
        public event Action MinuteChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void ResetForNewRun()
        {
            FormPulse = 0f;
            Score.Reset();
            MatchMinute = 1;
            GoalsEventsThisHalf = 0;
            OpponentName = "Прибой";
            _segmentTags.Clear();
            _segmentHasTwist = false;
            ScoreChanged?.Invoke();
            MinuteChanged?.Invoke();
        }

        public void BeginSegment(string segmentId, bool forceIrony = false)
        {
            _segmentTags.Clear();
            _segmentHasTwist = forceIrony;
        }

        public void MarkTwist() => _segmentHasTwist = true;

        public void ApplyChoiceTags(IEnumerable<string> tags)
        {
            if (tags == null) return;
            foreach (var raw in tags)
            {
                if (string.IsNullOrEmpty(raw)) continue;
                var tag = raw.Trim().ToLowerInvariant();
                _segmentTags.Add(tag);

                switch (tag)
                {
                    case "push_up":
                        FormPulse += PushUpWeight;
                        break;
                    case "push_down":
                        FormPulse -= PushDownWeight;
                        break;
                    case "twist":
                        _segmentHasTwist = true;
                        break;
                    // delay / arc_only: no immediate pulse
                }
            }
        }

        float SoftStatsBias()
        {
            if (SoftStatsService.Instance == null) return 0f;
            var s = SoftStatsService.Instance.State;
            // Weak bias: morale/focus help, anxiety/pain hurt.
            return (s.Morale + s.Focus) * 0.05f - (s.Anxiety + s.Pain) * 0.05f;
        }

        public OutcomePack BuildOutcomePack(string segmentId)
        {
            if (SoftStatsBiasEnabled)
                FormPulse += SoftStatsBias();

            var before = FormPulse;
            var pack = new OutcomePack
            {
                PackId = $"pack_{segmentId}_{Time.frameCount}",
                SegmentId = segmentId,
                PulseBefore = before
            };

            OutcomeType primary;
            if (_segmentHasTwist && FormPulse >= 0f)
            {
                // Irony: expected/neutral-good form → setback (slice seg2)
                primary = UnityEngine.Random.value < 0.55f ? OutcomeType.Miss : OutcomeType.GoalAgainst;
            }
            else if (FormPulse >= GoalForThreshold)
            {
                primary = OutcomeType.GoalFor;
                FormPulse -= GoalForConsume;
            }
            else if (FormPulse <= GoalAgainstThreshold)
            {
                primary = OutcomeType.GoalAgainst;
                FormPulse += GoalAgainstRecover;
            }
            else
            {
                primary = PickMidBand();
            }

            pack.Items.Add(MakeItem(primary));
            ClampRealism(pack);
            pack.PulseAfter = FormPulse;
            _segmentTags.Clear();
            _segmentHasTwist = false;
            return pack;
        }

        OutcomeType PickMidBand()
        {
            // Deterministic-ish from pulse sign for readability in playtests.
            if (FormPulse > 0.4f) return OutcomeType.DribbleWin;
            if (FormPulse < -0.4f) return OutcomeType.DribbleLose;
            return UnityEngine.Random.value < 0.5f ? OutcomeType.Miss : OutcomeType.BallOut;
        }

        static OutcomeItem MakeItem(OutcomeType type)
        {
            var item = new OutcomeItem { Type = type };
            switch (type)
            {
                case OutcomeType.GoalFor:
                    item.ScoreDeltaFor = 1;
                    break;
                case OutcomeType.GoalAgainst:
                    item.ScoreDeltaAgainst = 1;
                    break;
            }

            return item;
        }

        void ClampRealism(OutcomePack pack)
        {
            for (var i = 0; i < pack.Items.Count; i++)
            {
                var item = pack.Items[i];
                if (item.Type != OutcomeType.GoalFor && item.Type != OutcomeType.GoalAgainst)
                    continue;

                var wouldFor = Score.GoalsFor + item.ScoreDeltaFor;
                var wouldAgainst = Score.GoalsAgainst + item.ScoreDeltaAgainst;
                var events = GoalsEventsThisHalf;

                var overHalf = events >= MaxGoalsEventsPerHalf;
                var overFor = wouldFor > MaxGoalsForMatch;
                var overAgainst = wouldAgainst > MaxGoalsAgainstMatch;

                if (overHalf || overFor || overAgainst)
                {
                    item.Type = OutcomeType.Miss;
                    item.ScoreDeltaFor = 0;
                    item.ScoreDeltaAgainst = 0;
                    pack.RealismClamped = true;
                }
            }
        }

        public void ApplyPackToScore(OutcomePack pack)
        {
            if (pack?.Items == null) return;
            foreach (var item in pack.Items)
            {
                if (item.ScoreDeltaFor != 0 || item.ScoreDeltaAgainst != 0)
                {
                    Score.GoalsFor = Mathf.Max(0, Score.GoalsFor + item.ScoreDeltaFor);
                    Score.GoalsAgainst = Mathf.Max(0, Score.GoalsAgainst + item.ScoreDeltaAgainst);
                    if (item.Type == OutcomeType.GoalFor || item.Type == OutcomeType.GoalAgainst)
                        GoalsEventsThisHalf++;
                }
            }

            ScoreChanged?.Invoke();
        }

        public void SetMinute(int minute)
        {
            MatchMinute = Mathf.Max(0, minute);
            MinuteChanged?.Invoke();
        }

        public void NotifyHalfStarted()
        {
            GoalsEventsThisHalf = 0;
        }

        public ScoreSignal GetScoreSignal()
        {
            var diff = Score.GoalsFor - Score.GoalsAgainst;
            if (diff > 0) return ScoreSignal.Win;
            if (diff < 0) return ScoreSignal.Loss;
            return ScoreSignal.Draw;
        }

        public OutcomeType PrimaryOutcome(OutcomePack pack)
        {
            if (pack?.Items == null || pack.Items.Count == 0) return OutcomeType.Empty;
            foreach (var i in pack.Items)
            {
                if (i.Type != OutcomeType.Empty && i.Type != OutcomeType.BoostFlag)
                    return i.Type;
            }

            return pack.Items[0].Type;
        }

        public void Restore(float pulse, int gf, int ga, int minute, int eventsThisHalf)
        {
            FormPulse = pulse;
            Score.GoalsFor = gf;
            Score.GoalsAgainst = ga;
            MatchMinute = minute;
            GoalsEventsThisHalf = eventsThisHalf;
            ScoreChanged?.Invoke();
            MinuteChanged?.Invoke();
        }
    }
}
