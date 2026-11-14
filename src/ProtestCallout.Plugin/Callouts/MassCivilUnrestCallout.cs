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
            CalloutMessage = "Reports of a large public gathering";
            CalloutPosition = _sceneCenter;

            if (!base.OnBeforeCalloutDisplayed())
                return false;

            // Start preparing the scene before the player accepts and travels to it.
            // This gives the crowd time to populate before the player arrives.
            _scene = new CrowdScene();
            _scene.Begin(_sceneCenter, PluginSettings.Current);
            return true;
        }

        public override bool OnCalloutAccepted()
        {
            if (_usedThisSession)
            {
                CleanupScene();
                return false;
            }

            _usedThisSession = true;
            Game.DisplayNotification("A large gathering and a police line are already at the location.");
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            CleanupScene();
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
            CleanupScene();
            base.End();
        }

        private void CleanupScene()
        {
            if (_scene == null)
                return;

            _scene.Cleanup();
            _scene = null;
        }
    }
}
