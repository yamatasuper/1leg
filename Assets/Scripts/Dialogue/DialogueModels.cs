using System;
using System.Collections.Generic;

namespace NinetyMinutes.Dialogue
{
    [Serializable]
    public class DialogueChoice
    {
        public string Id;
        public string Text;
        public string NextNodeId;
        public string Tone;
        public List<string> Tags = new List<string>();
        public List<StatDelta> StatDeltas = new List<StatDelta>();
        public List<string> SetFlags = new List<string>();
    }

    [Serializable]
    public class StatDelta
    {
        public string Stat;
        public float Amount;
    }

    [Serializable]
    public class DialogueNode
    {
        public string Id;
        public string Speaker;
        public string Line;
        public string NextNodeId; // linear continue
        public List<DialogueChoice> Choices = new List<DialogueChoice>();
        public bool OpensJournalHint;
        public bool EndsDialogue;
    }

    [Serializable]
    public class DialogueGraph
    {
        public string Id;
        public string StartNodeId;
        public Dictionary<string, DialogueNode> Nodes = new Dictionary<string, DialogueNode>();

        /// <summary>If true, ending this dialogue resolves choice→score bridge and plays a match beat.</summary>
        public bool TriggersMatchBeat;
        public bool ForceIronyBeat;
        public string SegmentId;
        public string MatchBeatId;
        public int MinuteAfterBeat;
    }
}
