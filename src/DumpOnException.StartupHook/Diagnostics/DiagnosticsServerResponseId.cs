namespace DumpOnException.StartupHook.Diagnostics
{
    internal enum DiagnosticsServerResponseId : byte
    {
        OK = 0,
        Error = 255, // 0xFF
    }
}