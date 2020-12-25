namespace DumpOnException.StartupHook.Diagnostics
{
    internal enum DiagnosticsServerCommandSet : byte
    {
        Dump = 1,
        EventPipe = 2,
        Profiler = 3,
        Process = 4,
        Server = 255, // 0xFF
    }
}