using System;
using NinetyMinutes.Match;
using NinetyMinutes.Save;
using NinetyMinutes.Stats;
using UnityEngine;

namespace NinetyMinutes.Dialogue
{
    public sealed class DialogueRunner : MonoBehaviour
    {
        public static DialogueRunner Instance { get; private set; }

        public bool IsOpen { get; private set; }
        public DialogueGraph ActiveGraph { get; private set; }
        public DialogueNode ActiveNode { get; private set; }

        public event Action Opened;
        public event Action Closed;
        public event Action NodeChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void StartDialogue(DialogueGraph graph)
        {
            if (graph == null || IsOpen) return;
            ActiveGraph = graph;
            if (!graph.Nodes.TryGetValue(graph.StartNodeId, out var node))
            {
                Debug.LogError("Dialogue start node missing");
                return;
            }

            IsOpen = true;
            ActiveNode = node;
            if (SaveService.Instance != null)
                SaveService.Instance.SetBlockReason(SaveBlockReason.Dialogue);
            Opened?.Invoke();
            NodeChanged?.Invoke();
        }

        public void ContinueLinear()
        {
            if (!IsOpen || ActiveNode == null) return;
            if (ActiveNode.Choices != null && ActiveNode.Choices.Count > 0) return;

            if (ActiveNode.EndsDialogue || string.IsNullOrEmpty(ActiveNode.NextNodeId))
            {
                EndDialogue();
                return;
            }

            GoTo(ActiveNode.NextNodeId);
        }

        public void Choose(DialogueChoice choice)
        {
            if (!IsOpen || choice == null) return;

            if (SoftStatsService.Instance != null && choice.StatDeltas != null)
            {
                foreach (var d in choice.StatDeltas)
                    SoftStatsService.Instance.Apply(d.Stat, d.Amount);
            }

            if (choice.Tags != null && choice.Tags.Count > 0 && ChoiceScoreBridge.Instance != null)
                ChoiceScoreBridge.Instance.ApplyChoiceTags(choice.Tags);

            if (choice.SetFlags != null)
            {
                foreach (var f in choice.SetFlags)
                    SliceDialogues.Flags.Add(f);
            }

            if (string.IsNullOrEmpty(choice.NextNodeId))
            {
                EndDialogue();
                return;
            }

            GoTo(choice.NextNodeId);
        }

        void GoTo(string nodeId)
        {
            if (!ActiveGraph.Nodes.TryGetValue(nodeId, out var node))
            {
                EndDialogue();
                return;
            }

            ActiveNode = node;
            NodeChanged?.Invoke();
            if (node.EndsDialogue && (node.Choices == null || node.Choices.Count == 0) && string.IsNullOrEmpty(node.NextNodeId))
            {
                // show final line; wait for continue
            }
        }

        public void EndDialogue()
        {
            if (!IsOpen) return;
            IsOpen = false;
            ActiveNode = null;
            ActiveGraph = null;
            if (SaveService.Instance != null)
                SaveService.Instance.SetBlockReason(SaveBlockReason.None);
            Closed?.Invoke();

            if (Core.GameSession.Instance != null && SaveService.Instance != null)
                SaveService.Instance.TrySaveAuto("after_dialogue", Core.GameSession.Instance.BuildPayload(), out _);
        }
    }
}
