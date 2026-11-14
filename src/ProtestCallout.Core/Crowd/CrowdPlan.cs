using System;
using System.Collections.Generic;

namespace ProtestCallout.Core.Crowd
{
    public enum ParticipantRole
    {
        PeacefulProtester,
        AgitatedProtester,
        Bystander,
        ShopWitness,
        RoadsideObserver,
        RiotOfficer
    }

    public sealed class CrowdPlan
    {
        public IReadOnlyDictionary<ParticipantRole, int> Counts { get; }

        public int TotalCivilians { get; }

        private CrowdPlan(Dictionary<ParticipantRole, int> counts, int totalCivilians)
        {
            Counts = counts;
            TotalCivilians = totalCivilians;
        }

        /// <summary>Creates a balanced crowd composition without spawning game entities.</summary>
        public static CrowdPlan Create(int civilianCount, int riotOfficerCount)
        {
            if (civilianCount < 100)
                throw new ArgumentOutOfRangeException(nameof(civilianCount), "The scene requires at least 100 civilians.");
            if (riotOfficerCount < 1)
                throw new ArgumentOutOfRangeException(nameof(riotOfficerCount));

            int peaceful = (int)Math.Round(civilianCount * 0.52, MidpointRounding.AwayFromZero);
            int agitated = (int)Math.Round(civilianCount * 0.18, MidpointRounding.AwayFromZero);
            int witnesses = (int)Math.Round(civilianCount * 0.08, MidpointRounding.AwayFromZero);
            int observers = civilianCount - peaceful - agitated - witnesses;

            var counts = new Dictionary<ParticipantRole, int>
            {
                [ParticipantRole.PeacefulProtester] = peaceful,
                [ParticipantRole.AgitatedProtester] = agitated,
                [ParticipantRole.Bystander] = civilianCount - peaceful - agitated - witnesses - observers,
                [ParticipantRole.ShopWitness] = witnesses,
                [ParticipantRole.RoadsideObserver] = observers,
                [ParticipantRole.RiotOfficer] = riotOfficerCount
            };

            return new CrowdPlan(counts, civilianCount);
        }
    }
}
