namespace DumpOnException.StartupHook.Diagnostics
{
    /// <summary>
    /// Errors (HRESULT) returned for DiagnosticsServerCommandId.Error responses.
    /// </summary>
    internal enum DiagnosticsIpcError : uint
    {
        BadEncoding = 2148733828, // 0x80131384
        UnknownCommand = 2148733829, // 0x80131385
        UnknownMagic = 2148733830, // 0x80131386
        UnknownError = 2148733831, // 0x80131387
    }
}