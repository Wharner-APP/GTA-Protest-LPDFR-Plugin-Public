namespace ProtestCallout.Core.Configuration
{
    /// <summary>Configuration defaults. Game-specific INI loading belongs in the RPH adapter.</summary>
    public sealed class CalloutSettings
    {
        public int SessionChancePercent { get; set; } = 75;
        public int MinimumCrowdSize { get; set; } = 100;
        public int MaximumCrowdSize { get; set; } = 125;
        public bool EnableRoadBlockade { get; set; } = true;
        public bool EnableStagedIncidents { get; set; } = true;

        public void Validate()
        {
            if (SessionChancePercent < 0 || SessionChancePercent > 100)
                throw new System.ArgumentOutOfRangeException(nameof(SessionChancePercent));
            if (MinimumCrowdSize < 1 || MaximumCrowdSize < MinimumCrowdSize)
                throw new System.ArgumentOutOfRangeException(nameof(MinimumCrowdSize));
        }
    }
}
