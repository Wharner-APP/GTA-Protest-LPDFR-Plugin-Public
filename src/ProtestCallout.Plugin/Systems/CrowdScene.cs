using System;
using System.Collections.Generic;
using ProtestCallout.Core.Crowd;
using Rage;

namespace ProtestCallout.Plugin.Systems
{
    /// <summary>
    /// Creates a staged public-gathering scene. Civilians use low-complexity ambient
    /// tasks; the police line is deliberately stationary and receives no combat tasks.
    /// </summary>
    internal sealed class CrowdScene
    {
        private static readonly string[] PeacefulModels =
        {
            "a_m_y_hipster_01", "a_f_y_hipster_01", "a_m_y_business_01"
        };

        private static readonly string[] AgitatedModels =
        {
            "a_m_y_roadcyc_01", "a_m_y_hipster_01"
        };

        private static readonly string[] BystanderModels =
        {
            "a_f_y_tourist_01", "a_m_y_business_01", "a_f_y_hipster_01"
        };

        private readonly object _sync = new object();
        private readonly List<Ped> _spawnedPeds = new List<Ped>();
        private readonly Random _random = new Random();
        private volatile bool _cleanupRequested;
        private int _nextCivilianIndex;

        public void Begin(Vector3 center)
        {
            _cleanupRequested = false;
            GameFiber.StartNew(() => SpawnScene(center), "GTAProtestCallout.SceneSpawner");
        }

        private void SpawnScene(Vector3 center)
        {
            try
            {
                CrowdPlan plan = CrowdPlan.Create(100, 11);

                SpawnGroup(center, plan.Counts[ParticipantRole.PeacefulProtester], PeacefulModels, 10);
                SpawnGroup(center, plan.Counts[ParticipantRole.AgitatedProtester], AgitatedModels, 30);
                SpawnGroup(center, plan.Counts[ParticipantRole.Bystander], BystanderModels, 45);
                SpawnGroup(center, plan.Counts[ParticipantRole.ShopWitness], BystanderModels, 0);
                SpawnGroup(center, plan.Counts[ParticipantRole.RoadsideObserver], BystanderModels, 15);

                // Officers hold a single line in front of the gathering. They receive
                // only a hold-position task, never a combat or pursuit task.
                for (int i = 0; i < plan.Counts[ParticipantRole.RiotOfficer]; i++)
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

        private void SpawnGroup(Vector3 center, int count, string[] models, int wanderChance)
        {
            for (int i = 0; i < count; i++)
            {
                if (_cleanupRequested)
                    return;

                int cell = _nextCivilianIndex++;
                int row = cell / 10;
                int column = cell % 10;
                float x = center.X + (column - 4.5f) * 2.4f;
                float y = center.Y + (row - 4.5f) * 2.4f;
                string model = models[_random.Next(models.Length)];
                Ped ped = CreatePed(model, new Vector3(x, y, center.Z), _random.Next(0, 360));

                if (ped != null)
                {
                    if (_random.Next(0, 100) < wanderChance)
                        ped.Tasks.Wander();
                    else
                        ped.Tasks.StandStill(-1);
                }

                GameFiber.Wait(35);
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
