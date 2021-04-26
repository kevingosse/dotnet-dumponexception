using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using Microsoft.Diagnostics.NETCore.Client;
// ReSharper disable UnusedType.Global

namespace DumpOnException.Dumpster
{
    internal class Listener
    {
        private static DiagnosticsClient? _client;
        private static int _count;
        private static int _running;
        private static Thread _memoryThresholdThread;
        private static Process _currentProcess;
    
        public Listener()
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;

            if (Settings.MemoryThreshold > 0)
            {
                _currentProcess = Process.GetCurrentProcess();
                _memoryThresholdThread = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(TimeSpan.FromMinutes(1));
                        
                        _currentProcess.Refresh();
                        
                        // MemoryThreshold in Megabytes, 1MB = 1048576 bytes (windows)
                        if (_currentProcess.PrivateMemorySize64 >= ((long)Settings.MemoryThreshold) * 1048576)
                        {
                            WriteDump("Memory_Threshold");
                        }
                    }
                });
                _memoryThresholdThread.IsBackground = true;
                _memoryThresholdThread.Name = "MemoryThreshold Thread";
                _memoryThresholdThread.Start();
            }
        }

        private static void CurrentDomainOnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (!Settings.FilterRegex.IsMatch(e.Exception.GetType().Name))
            {
                return;
            }
        
            WriteDump(e.Exception.GetType().Name);
        }

        private static void WriteDump(string name)
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) == 0)
            {
                try
                {
                    if (Settings.AttachDebugger && Debugger.Launch())
                    {
                        Debugger.Break();
                    }
                    
                    string path = Path.Combine(Settings.Directory, $"Dump_{name}_{++_count}_{Settings.ProcessId}.dmp");
                    _client ??= new DiagnosticsClient(Settings.ProcessId);
                    _client.WriteDump(DumpType.Full, path, true);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    Interlocked.Exchange(ref _running, 0);
                }
            }
        }
    }
}