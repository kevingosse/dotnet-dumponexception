using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
// ReSharper disable MemberCanBePrivate.Global

namespace DumpOnException.Dumpster
{
    internal static class Settings
    {
        public static int ProcessId { get; }
        public static string Filter { get; }
        public static string Directory { get; }
        public static Regex FilterRegex { get; }
        public static bool AttachDebugger { get; }
        public static int MemoryThreshold { get; }
        public static int PeriodicDumpInMinutes { get; }

        static Settings()
        {
            ProcessId = Process.GetCurrentProcess().Id;
            Filter = GetEnvironmentValue("DOE_FILTER", ".*");
            Directory = GetEnvironmentValue("DOE_DIRECTORY", string.Empty);
            AttachDebugger = GetEnvironmentValue("DOE_ATTACH", "0") == "1";
            FilterRegex = new Regex(Filter, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string strMemoryThreshold = GetEnvironmentValue("DOE_MEMTHRESHOLD", string.Empty);
            if (int.TryParse(strMemoryThreshold, out int memThreshold))
            {
                MemoryThreshold = memThreshold;
            }

            string strPeriodicDumpInMinutes = GetEnvironmentValue("DOE_PERIODICMIN", string.Empty);
            if (int.TryParse(strPeriodicDumpInMinutes, out int intPeriodicDumpInMinutes))
            {
                PeriodicDumpInMinutes = intPeriodicDumpInMinutes;
            }

            static string GetEnvironmentValue(string name, string defaultValue)
            {
                try
                {
                    return Environment.GetEnvironmentVariable(name) ?? defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }
        }
    }
}