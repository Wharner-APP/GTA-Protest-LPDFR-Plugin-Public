using System;
using System.Collections.Generic;
using Rage;

namespace ProtestCallout.Plugin.Systems
{
    /// <summary>
    /// Creates a staged public-gathering scene. The crowd uses low-complexity ambient
    /// tasks; the police line is deliberately stationary and is not given combat tasks.
    /// </summary>
    internal sealed class CrowdScene
    {
        private static readonly string[] CivilianModels =
        {
            "a_m_y_hipster_01",
            "a_f_y_hipster_01",
            "a_m_y_business_01",
            "a_f_y_tourist_01",
            "a_m_y_roadcyc_01"
        };

        private readonly object _sync = new object();
        private readonly List<Ped> _spawnedPeds = new List<Ped>();
        private readonly Random _random = new Random();
        private volatile bool _cleanupRequested;

        public void Begin(Vector3 center)
        {
            _cleanupRequested = false;
            GameFiber.StartNew(() => SpawnScene(center), "GTAProtestCallout.SceneSpawner");
        }

        private void SpawnScene(Vector3 center)
        {
            try
            {
                // 10 x 10 = 100 civilians. Spawning is spread over several seconds
                // to avoid one large synchronous burst of entity creation.
                for (int row = 0; row < 10; row++)
                {
                    for (int column = 0; column < 10; column++)
                    {
                        if (_cleanupRequested)
                            return;

                        float x = center.X + (column - 4.5f) * 2.4f;
                        float y = center.Y + (row - 4.5f) * 2.4f;
                        string model = CivilianModels[_random.Next(CivilianModels.Length)];
                        Ped ped = CreatePed(model, new Vector3(x, y, center.Z), _random.Next(0, 360));

                        if (ped != null)
                        {
                            // Most participants remain in place; a smaller portion
                            // wander locally to keep the scene from looking frozen.
                            if (_random.Next(0, 100) < 25)
                                ped.Tasks.Wander();
                            else
                                ped.Tasks.StandStill(-1);
                        }

                        GameFiber.Wait(35);
                    }
                }

                // Officers are placed in a line in front of the gathering. They are
                // instructed only to hold position; no attack or pursuit task is issued.
                for (int i = 0; i < 11; i++)
                {
                    if (_cleanupRequested)
                        return;

                    float x = center.X + (i - 5) * 2.8f;
                    float y = center.Y - 17f;
                    Ped officer = CreatePed("s_m_y_cop_01", new Vector3(x, y, center.Z), 0);

                    if (officer != null)
                        officer.Tasks.StandStill(-1);

                    GameFiber.Wait(45);
                }

                Game.LogTrivial("[GTAProtestCallout] Scene population pass completed.");
            }
            catch (Exception ex)
            {
                Game.LogTrivial("[GTAProtestCallout] Scene spawn error: " + ex);
            }
        }

        private Ped CreatePed(string modelName, Vector3 position, int heading)
        {
            if (_cleanupRequested)
                return null;

            try
            {
                Ped ped = new Ped(modelName, position, heading);
                if (ped == null || !ped.Exists())
                    return null;

                ped.IsPersistent = true;
                ped.BlockPermanentEvents = true;

                lock (_sync)
                {
                    if (_cleanupRequested)
                    {
                        if (ped.Exists())
                            ped.Delete();
                        return null;
                    }

                    _spawnedPeds.Add(ped);
                }

                return ped;
            }
            catch (Exception ex)
            {
                Game.LogTrivial("[GTAProtestCallout] Could not create ped '" + modelName + "': " + ex.Message);
                return null;
            }
        }

        public void Cleanup()
        {
            _cleanupRequested = true;

            Ped[] snapshot;
            lock (_sync)
            {
                snapshot = _spawnedPeds.ToArray();
                _spawnedPeds.Clear();
            }

            foreach (Ped ped in snapshot)
            {
                try
                {
                    if (ped != null && ped.Exists())
                        ped.Delete();
                }
                catch (Exception ex)
                {
                    Game.LogTrivial("[GTAProtestCallout] Entity cleanup warning: " + ex.Message);
                }
            }
        }
    }
}
