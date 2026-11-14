using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using ProtestCallout.Plugin.Configuration;
using ProtestCallout.Plugin.Systems;

namespace ProtestCallout.Plugin.Callouts
{
    [CalloutInfo("Mass Civil Unrest", CalloutProbability.High)]
    public sealed class MassCivilUnrestCallout : Callout
    {
        private static bool _usedThisSession;
        private Vector3 _sceneCenter;
        private CrowdScene _scene;

        public override bool OnBeforeCalloutDisplayed()
        {
            if (_usedThisSession)
                return false;

            _sceneCenter = World.GetNextPositionOnStreet(
                Game.LocalPlayer.Character.Position.Around(350f));

            ShowCalloutAreaBlipBeforeAccepting(_sceneCenter, 90f);
            AddMinimumDistanceCheck(40f, _sceneCenter);
            CalloutMessage = "Reports of a large public disturbance";
            CalloutPosition = _sceneCenter;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (_usedThisSession)
                return false;

            _usedThisSession = true;
            _scene = new CrowdScene();
            _scene.Begin(_sceneCenter, PluginSettings.Current);

            Game.DisplayNotification("A large crowd and a police line are at the reported location.");
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (_scene != null)
                _scene.Cleanup();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (Game.LocalPlayer.Character == null || Game.LocalPlayer.Character.IsDead)
                End();
        }

        public override void End()
        {
            if (_scene != null)
            {
                _scene.Cleanup();
                _scene = null;
            }

            base.End();
        }
    }
}
