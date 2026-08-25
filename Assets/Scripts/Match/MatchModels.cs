using System;
using System.Collections.Generic;

namespace NinetyMinutes.Match
{
    public enum OutcomeType
    {
        Empty = 0,
        GoalFor = 1,
        GoalAgainst = 2,
        DribbleWin = 3,
        DribbleLose = 4,
        Miss = 5,
        BallOut = 6,
        BoostFlag = 7
    }

    public enum ScoreSignal
    {
        Draw = 0,
        Win = 1,
        Loss = 2
    }

    [Serializable]
    public sealed class OutcomeItem
    {
        public OutcomeType Type;
        public int ScoreDeltaFor;
        public int ScoreDeltaAgainst;
        public bool BoostFlag;
    }

    [Serializable]
    public sealed class OutcomePack
    {
        public string PackId;
        public string SegmentId;
        public List<OutcomeItem> Items = new List<OutcomeItem>();
        public bool RealismClamped;
        public float PulseBefore;
        public float PulseAfter;
    }

    [Serializable]
    public sealed class MatchScore
    {
        public int GoalsFor;
        public int GoalsAgainst;

        public void Reset()
        {
            GoalsFor = 0;
            GoalsAgainst = 0;
        }
    }

    public sealed class MatchBeatRequest
    {
        public string BeatId;
        public string SegmentId;
        public OutcomePack Pack;
        public int MinuteAfter;
        public MatchScore ScoreBefore;
        public MatchScore ScoreAfter;
        public string OpponentName = "Прибой";
    }
}
