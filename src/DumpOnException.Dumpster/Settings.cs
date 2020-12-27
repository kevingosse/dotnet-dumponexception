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

        static Settings()
        {
            ProcessId = Process.GetCurrentProcess().Id;
            Filter = GetEnvironmentValue("DOE_FILTER", ".*");
            Directory = GetEnvironmentValue("DOE_DIRECTORY", string.Empty);
            FilterRegex = new Regex(Filter, RegexOptions.Compiled | RegexOptions.IgnoreCase);

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