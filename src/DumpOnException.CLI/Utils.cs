using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DumpOnException.CLI
{
    internal static class Utils
    {
        public static ProcessStartInfo GetProcessStartInfo(string filename, IDictionary<string, string?>? environmentVariables)
        {
            ProcessStartInfo processInfo = new(filename)
            {
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
            };

            foreach (object? item in Environment.GetEnvironmentVariables())
            {
                if (item is DictionaryEntry entry)
                {
                    processInfo.Environment[entry.Key.ToString() ?? string.Empty] = entry.Value?.ToString() ?? string.Empty;
                }
            }

            if (environmentVariables != null)
            {
                foreach (KeyValuePair<string, string?> item in environmentVariables)
                {
                    processInfo.Environment[item.Key] = item.Value;
                }
            }

            return processInfo;
        }
        public static int RunProcess(ProcessStartInfo startInfo, CancellationToken cancellationToken)
        {
            try
            {
                using (Process childProcess = new())
                {
                    childProcess.StartInfo = startInfo;
                    childProcess.EnableRaisingEvents = true;
                    childProcess.Start();

                    using (cancellationToken.Register(() =>
                    {
                        try
                        {
                            childProcess.Kill();
                        }
                        catch
                        {
                            //
                        }
                    }))
                    {
                        childProcess.WaitForExit();
                        return cancellationToken.IsCancellationRequested ? -1 : childProcess.ExitCode;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return -1;
        }
        public static string GetEnvironmentValue(string name, string defaultValue)
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