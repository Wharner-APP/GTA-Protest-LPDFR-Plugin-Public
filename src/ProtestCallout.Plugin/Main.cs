using System;
using LSPD_First_Response.Mod.API;
using Rage;
using ProtestCallout.Core.Session;

namespace ProtestCallout.Plugin
{
    /// <summary>
    /// LSPDFR plugin entry point. Keep this class named Main for LSPDFR plugin discovery.
    /// </summary>
    public sealed class Main : Plugin
    {
        private SessionCalloutGate _sessionGate;
        private bool _calloutRegistered;

        public override void Initialize()
        {
            _sessionGate = new SessionCalloutGate(75);
            Functions.OnOnDutyStateChanged += OnDutyStateChanged;
            Game.LogTrivial("[GTAProtestCallout] Loaded. Waiting for the first on-duty transition.");
        }

        private void OnDutyStateChanged(bool onDuty)
        {
            if (!onDuty || _calloutRegistered)
                return;

            if (!_sessionGate.ShouldSchedule)
            {
                Game.LogTrivial("[GTAProtestCallout] The 75% session roll did not schedule this event.");
                return;
            }

            Functions.RegisterCallout(typeof(Callouts.MassCivilUnrestCallout));
            _calloutRegistered = true;
            Game.LogTrivial("[GTAProtestCallout] Mass Civil Unrest callout registered for this session.");
        }

        public override void Finally()
        {
            Functions.OnOnDutyStateChanged -= OnDutyStateChanged;
            Game.LogTrivial("[GTAProtestCallout] Unloaded.");
        }
    }
}
