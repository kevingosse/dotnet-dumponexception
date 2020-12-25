using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using DumpOnException.StartupHook;
using DumpOnException.StartupHook.Diagnostics;

internal class StartupHook
{
    private static DiagnosticsClient _client;
    private static int _count;
    private static int _running;
    
    public static void Initialize()
    {
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
    }

    private static void CurrentDomainOnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
    {
        if (!Settings.FilterRegex.IsMatch(e.Exception.GetType().Name))
        {
            return;
        }
        
        if (Interlocked.CompareExchange(ref _running, 1, 0) == 0)
        {
            try
            {
                CreateDump(e.Exception);
            }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
            }
        }
    }

    private static void CreateDump(Exception exception)
    {
        try
        {
            _client ??= new DiagnosticsClient(Settings.ProcessId);
            string path = Path.Combine(Settings.Directory, $"Dump_{exception.GetType().Name}_{++_count}_{Settings.ProcessId}.dmp");
            _client.WriteDump(DumpType.Full, path, true);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}