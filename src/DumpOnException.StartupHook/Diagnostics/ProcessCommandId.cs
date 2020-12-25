namespace DumpOnException.StartupHook.Diagnostics
{
    internal enum ProcessCommandId : byte
    {
        GetProcessInfo,
        ResumeRuntime,
        GetProcessEnvironment,
    }
}