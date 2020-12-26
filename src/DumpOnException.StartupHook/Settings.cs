using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DumpOnException.StartupHook
{
    internal static class Settings
    {
        public static int ProcessId { get; private set; }
        public static string Filter { get; private set; }
        public static string Directory { get; private set; }
        public static Regex FilterRegex { get; private set; }

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