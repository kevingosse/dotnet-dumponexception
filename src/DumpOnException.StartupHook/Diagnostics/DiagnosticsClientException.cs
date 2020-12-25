using System;

namespace DumpOnException.StartupHook.Diagnostics
{
    public class DiagnosticsClientException : Exception
    {
        public DiagnosticsClientException(string msg) : base(msg) {}
    }
    
    // When a certian command is not supported by either the library or the target process' runtime
    public class UnsupportedProtocolException : DiagnosticsClientException
    {
        public UnsupportedProtocolException(string msg) : base(msg) {}
    }

    // When the runtime is no longer availble for attaching.
    public class ServerNotAvailableException : DiagnosticsClientException
    {
        public ServerNotAvailableException(string msg) : base(msg) {}
    }

    // When the runtime responded with an error
    public class ServerErrorException : DiagnosticsClientException
    {
        public ServerErrorException(string msg): base(msg) {}
    }

    // When the runtime doesn't support the command
    public class UnsupportedCommandException : ServerErrorException
    {
        public UnsupportedCommandException(string msg): base(msg) {}
    }
}