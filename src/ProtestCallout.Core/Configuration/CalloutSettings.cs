namespace ProtestCallout.Core.Configuration
{
    /// <summary>Validated defaults shared by the core and the game adapter.</summary>
    public sealed class CalloutSettings
    {
        public int SessionChancePercent { get; set; } = 75;
        public int MinimumCrowdSize { get; set; } = 100;
        public int MaximumCrowdSize { get; set; } = 125;
        public int PoliceLineCount { get; set; } = 11;
        public int SpawnIntervalMs { get; set; } = 35;
        public int CivilianWanderChancePercent { get; set; } = 25;

        public void Validate()
        {
            if (SessionChancePercent < 0 || SessionChancePercent > 100)
                throw new System.ArgumentOutOfRangeException(nameof(SessionChancePercent));
            if (MinimumCrowdSize < 100 || MaximumCrowdSize < MinimumCrowdSize || MaximumCrowdSize > 150)
                throw new System.ArgumentOutOfRangeException(nameof(MinimumCrowdSize),
                    "Civilian counts must be between 100 and 150, with maximum >= minimum.");
            if (PoliceLineCount < 1 || PoliceLineCount > 40)
                throw new System.ArgumentOutOfRangeException(nameof(PoliceLineCount));
            if (SpawnIntervalMs < 10 || SpawnIntervalMs > 250)
                throw new System.ArgumentOutOfRangeException(nameof(SpawnIntervalMs));
            if (CivilianWanderChancePercent < 0 || CivilianWanderChancePercent > 100)
                throw new System.ArgumentOutOfRangeException(nameof(CivilianWanderChancePercent));
        }
    }
}
