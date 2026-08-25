using System;

namespace NinetyMinutes.Save
{
    public enum SaveKind
    {
        Manual = 0,
        Auto = 1
    }

    public enum SaveBlockReason
    {
        None = 0,
        Dialogue = 1,
        MatchPresentation = 2,
        TraumaCut = 3
    }

    [Serializable]
    public class SaveMeta
    {
        public string slotId;
        public SaveKind kind;
        public int index;
        public long unixTimestamp;
        public float playtimeSec;
        public string actId;
        public string timeMode;
        public string locationId;
        public int matchMinute = -1;
        public int goalsFor = -1;
        public int goalsAgainst = -1;
        public bool traumaTriggered;
        public bool endingLocked;
        public string summaryLabel;
    }

    [Serializable]
    public class SavePayload
    {
        public int schemaVersion = 1;
        public SaveMeta meta = new SaveMeta();

        // Stub fields — Sprint 1+ will fill these.
        public string sessionPhase = "menu";
        public bool campaignStarted;
        public string heroName = "Бардин";
        public string opponentName = "Прибой";
        public string notes;

        // Sprint 1 soft stats snapshot
        public float softMorale;
        public float softEnergy;
        public float softStrength;
        public float softFocus;
        public float softPain;
        public float softAnxiety;
        public float playerX;
        public float playerY;

        // Sprint 2 match bridge
        public float formPulse;
        public int goalsFor;
        public int goalsAgainst;
        public int matchMinute = 1;
        public int goalsEventsThisHalf;
        public string flagsCsv = "";

        // Sprint 3 spine
        public string slicePhase = "None";
        public string endingRoute = "None";
        public float lifeScore;
    }

    [Serializable]
    public class SaveSlotFile
    {
        public SavePayload payload;
    }
}
