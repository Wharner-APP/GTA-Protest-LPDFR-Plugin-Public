using System;
using System.Collections.Generic;
using System.IO;
using ProtestCallout.Core.Configuration;
using Rage;

namespace ProtestCallout.Plugin.Configuration
{
    internal static class PluginSettings
    {
        public static CalloutSettings Current { get; private set; } = new CalloutSettings();

        public static void Load()
        {
            var settings = new CalloutSettings();
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Plugins",
                "LSPDFR",
                "GTAProtestCallout.ini");

            try
            {
                if (File.Exists(path))
                {
                    Dictionary<string, string> values = ReadValues(path);

                    settings.SessionChancePercent = ReadInt(values, "SessionChancePercent", settings.SessionChancePercent);
                    settings.MinimumCrowdSize = ReadInt(values, "MinimumCrowdSize", settings.MinimumCrowdSize);
                    settings.MaximumCrowdSize = ReadInt(values, "MaximumCrowdSize", settings.MaximumCrowdSize);
                    settings.PoliceLineCount = ReadInt(values, "PoliceLineCount", settings.PoliceLineCount);
                    settings.SpawnIntervalMs = ReadInt(values, "SpawnIntervalMs", settings.SpawnIntervalMs);
                    settings.CivilianWanderChancePercent = ReadInt(values, "CivilianWanderChancePercent", settings.CivilianWanderChancePercent);
                    settings.EnableRoadBlockade = ReadBool(values, "EnableRoadBlockade", settings.EnableRoadBlockade);
                    settings.EnableStagedIncidents = ReadBool(values, "EnableStagedIncidents", settings.EnableStagedIncidents);
                }
                else
                {
                    Game.LogTrivial("[GTAProtestCallout] INI not found; using built-in defaults: " + path);
                }

                settings.Validate();
                Current = settings;
                Game.LogTrivial("[GTAProtestCallout] Settings loaded. Session chance: "
                    + Current.SessionChancePercent + "%; crowd: "
                    + Current.MinimumCrowdSize + "-" + Current.MaximumCrowdSize
                    + "; police line: " + Current.PoliceLineCount + ".");
            }
            catch (Exception ex)
            {
                Current = new CalloutSettings();
                Game.LogTrivial("[GTAProtestCallout] Invalid INI; using safe defaults. " + ex.Message);
            }
        }

        private static Dictionary<string, string> ReadValues(string path)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string rawLine in File.ReadAllLines(path))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("["))
                    continue;

                int separator = line.IndexOf('=');
                if (separator <= 0)
                    continue;

                string key = line.Substring(0, separator).Trim();
                string value = line.Substring(separator + 1).Trim();
                values[key] = value;
            }

            return values;
        }

        private static int ReadInt(Dictionary<string, string> values, string key, int fallback)
        {
            string value;
            int parsed;
            return values.TryGetValue(key, out value) && int.TryParse(value, out parsed)
                ? parsed
                : fallback;
        }

        private static bool ReadBool(Dictionary<string, string> values, string key, bool fallback)
        {
            string value;
            bool parsed;
            return values.TryGetValue(key, out value) && bool.TryParse(value, out parsed)
                ? parsed
                : fallback;
        }
    }
}
