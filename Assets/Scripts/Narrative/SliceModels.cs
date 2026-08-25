namespace NinetyMinutes.Narrative
{
    public enum SlicePhase
    {
        None = 0,
        Intro = 1,
        Training = 2,
        /// <summary>All past dialogues on the street; match fires only when all are done.</summary>
        StreetLife = 3,
        Match = 4,
        EndingCard = 9,
        Credits = 10,
        Finished = 11
    }

    public enum EndingRoute
    {
        None = 0,
        Good = 1,
        Mid = 2,
        Bad = 3
    }
}
