using System;

namespace ProtestCallout.Core.Session
{
    /// <summary>Rolls once per game session; repeated polling never rerolls.</summary>
    public sealed class SessionCalloutGate
    {
        private readonly int _chancePercent;
        private readonly Random _random;
        private bool? _shouldSchedule;

        public SessionCalloutGate(int chancePercent, Random? random = null)
        {
            if (chancePercent < 0 || chancePercent > 100)
                throw new ArgumentOutOfRangeException(nameof(chancePercent));
            _chancePercent = chancePercent;
            _random = random ?? new Random();
        }

        public bool ShouldSchedule
        {
            get
            {
                if (!_shouldSchedule.HasValue)
                    _shouldSchedule = _random.Next(0, 100) < _chancePercent;
                return _shouldSchedule.Value;
            }
        }

        public void ResetForNewSession() => _shouldSchedule = null;
    }
}
