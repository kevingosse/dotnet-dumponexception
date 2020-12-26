using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DumpOnException.CLI
{
    internal class Utils
    {
        public static ProcessStartInfo GetProcessStartInfo(string filename, IDictionary<string, string> environmentVariables)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(filename)
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory,
            };

            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                processInfo.Environment[item.Key.ToString()] = item.Value.ToString();
            }

            if (environmentVariables != null)
            {
                foreach (KeyValuePair<string, string> item in environmentVariables)
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
                using (Process childProcess = new Process())
                {
                    childProcess.StartInfo = startInfo;
                    childProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            Console.WriteLine(e.Data);
                    };
                    childProcess.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            Console.Error.WriteLine(e.Data);
                    };
                    childProcess.EnableRaisingEvents = true;
                    childProcess.Start();
                    childProcess.BeginOutputReadLine();
                    childProcess.BeginErrorReadLine();

                    using (cancellationToken.Register(() =>
                    {
                        try
                        {
                            if (childProcess != null)
                            {
                                childProcess.StandardInput.Close();
                                childProcess.Kill();
                            }
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
    }
}