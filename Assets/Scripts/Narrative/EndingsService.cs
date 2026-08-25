using NinetyMinutes.Dialogue;
using NinetyMinutes.Match;
using NinetyMinutes.Stats;
using UnityEngine;

namespace NinetyMinutes.Narrative
{
    public sealed class EndingsService : MonoBehaviour
    {
        public static EndingsService Instance { get; private set; }

        public EndingRoute LockedRoute { get; private set; } = EndingRoute.None;
        public float LastLifeScore { get; private set; }
        public bool IsLocked => LockedRoute != EndingRoute.None;

        const float WArcs = 0.35f;
        const float WSoft = 0.30f;
        const float WFlags = 0.20f;
        const float WMatch = 0.15f;
        const float GoodThreshold = 1.0f;
        const float BadThreshold = -1.0f;

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
            LockedRoute = EndingRoute.None;
            LastLifeScore = 0f;
        }

        public EndingRoute ResolveAndLock()
        {
            if (IsLocked) return LockedRoute;

            var sMatch = ScoreMatch();
            var sSoft = ScoreSoft();
            var sFlags = ScoreFlags();
            var sArcs = ScoreArcs();

            LastLifeScore = WArcs * sArcs + WSoft * sSoft + WFlags * sFlags + WMatch * sMatch;
            var route = Band(LastLifeScore);

            // Anti-dominance: score alone shouldn't crown/doom
            if (sMatch > 0.5f && sSoft < -0.4f && sArcs < -0.2f && route == EndingRoute.Good)
                route = EndingRoute.Mid;
            if (sMatch < -0.5f && sSoft > 0.4f && sArcs > 0.2f && route == EndingRoute.Bad)
                route = EndingRoute.Mid;

            LockedRoute = route;
            Debug.Log($"[90 минут] Ending locked · {route} · life={LastLifeScore:F2} (arcs={sArcs:F2} soft={sSoft:F2} flags={sFlags:F2} match={sMatch:F2})");
            return route;
        }

        public void Restore(EndingRoute route, float lifeScore)
        {
            LockedRoute = route;
            LastLifeScore = lifeScore;
        }

        float ScoreMatch()
        {
            if (ChoiceScoreBridge.Instance == null) return 0f;
            switch (ChoiceScoreBridge.Instance.GetScoreSignal())
            {
                case ScoreSignal.Win: return 1f;
                case ScoreSignal.Loss: return -1f;
                default: return 0f;
            }
        }

        float ScoreSoft()
        {
            if (SoftStatsService.Instance == null) return 0f;
            var s = SoftStatsService.Instance.State;
            var v = (s.Morale + s.Focus - s.Anxiety - s.Pain) / 10f;
            return Mathf.Clamp(v, -1f, 1f);
        }

        float ScoreFlags()
        {
            var f = SliceDialogues.Flags;
            float v = 0f;
            if (f.Contains("chose_self")) v += 0.9f;
            if (f.Contains("chose_numb")) v -= 0.9f;
            if (f.Contains("told_glock_truth")) v -= 0.5f;
            if (f.Contains("promised_silence_held")) v += 0.35f;
            if (f.Contains("training_skipped")) v -= 0.25f;
            return Mathf.Clamp(v, -1f, 1f);
        }

        float ScoreArcs()
        {
            var f = SliceDialogues.Flags;
            float v = 0f;
            if (f.Contains("rel_glock_up")) v += 0.7f;
            if (f.Contains("rel_glock_down")) v -= 0.7f;
            return Mathf.Clamp(v, -1f, 1f);
        }

        static EndingRoute Band(float life)
        {
            if (life >= GoodThreshold) return EndingRoute.Good;
            if (life <= BadThreshold) return EndingRoute.Bad;
            return EndingRoute.Mid;
        }

        public static string Title(EndingRoute r)
        {
            switch (r)
            {
                case EndingRoute.Good: return "Ты был здесь";
                case EndingRoute.Bad: return "Ты ушёл";
                default: return "Сегодня — было";
            }
        }

        public static string Body(EndingRoute r)
        {
            switch (r)
            {
                case EndingRoute.Good:
                    return "Ты выиграл. Ты был богат. Ты был беден. Но ты был здесь. И этого достаточно.";
                case EndingRoute.Bad:
                    return "Ты ушёл. Ты больше никогда не вернёшься. Стадион останется. Ты — нет.";
                default:
                    return "Ты остался на поле после свистка. Ты знал: завтра этого не будет. Но сегодня — было.";
            }
        }
    }
}
