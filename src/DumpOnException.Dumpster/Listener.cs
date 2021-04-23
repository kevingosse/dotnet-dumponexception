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
        private static Timer _memoryThresholdTimer;
        private static Process _currentProcess;
    
        public Listener()
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;

            if (Settings.MemoryThreshold > 0)
            {
                _currentProcess = Process.GetCurrentProcess();
                _memoryThresholdTimer = new Timer(state =>
                {
                    _currentProcess.Refresh();

                    if (_currentProcess.PrivateMemorySize64 >= Settings.MemoryThreshold)
                    {
                        WriteDump("Memory_Threshold");
                    }
                }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
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