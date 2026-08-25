using NinetyMinutes.Dialogue;
using NinetyMinutes.Match;
using NinetyMinutes.Narrative;
using NinetyMinutes.Save;
using NinetyMinutes.Stats;
using NinetyMinutes.UI;
using NinetyMinutes.World;
using UnityEngine;

namespace NinetyMinutes.Core
{
    public static class GameBootstrap
    {
        const string RootName = "NinetyMinutes_Systems";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindObjectOfType<GameSession>() != null) return;

            var root = new GameObject(RootName);
            Object.DontDestroyOnLoad(root);
            root.AddComponent<GameSession>();
            root.AddComponent<SaveService>();
            root.AddComponent<SoftStatsService>();
            root.AddComponent<ChoiceScoreBridge>();
            root.AddComponent<MatchFrameUI>();
            root.AddComponent<MatchPresentation>();
            root.AddComponent<MatchBeatDirector>();
            root.AddComponent<EndingsService>();
            root.AddComponent<SliceDirector>();
            root.AddComponent<DialogueRunner>();
            root.AddComponent<DialogueUI>();
            root.AddComponent<JournalUI>();
            root.AddComponent<EndingCardUI>();
            root.AddComponent<WorldController>();
            root.AddComponent<AppShellMenu>();

            Debug.Log("[90 минут] Sprint 3 bootstrap OK — full slice spine.");
        }
    }
}
