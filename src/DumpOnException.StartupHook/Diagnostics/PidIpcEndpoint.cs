using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace DumpOnException.StartupHook.Diagnostics
{
    internal class PidIpcEndpoint : IpcEndpoint
    {
        public static string IpcRootPath { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"\\.\pipe\" : Path.GetTempPath();
        public static string DiagnosticsPortPattern { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"^dotnet-diagnostic-(\d+)$" : @"^dotnet-diagnostic-(\d+)-(\d+)-socket$";

        private int _pid;

        /// <summary>
        /// Creates a reference to a .NET process's IPC Transport
        /// using the default rules for a given pid
        /// </summary>
        /// <param name="pid">The pid of the target process</param>
        /// <returns>A reference to the IPC Transport</returns>
        public PidIpcEndpoint(int pid)
        {
            _pid = pid;
        }

        public override Stream Connect(TimeSpan timeout)
        {
            string address = GetDefaultAddress();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var namedPipe = new NamedPipeClientStream(
                    ".",
                    address,
                    PipeDirection.InOut,
                    PipeOptions.None,
                    TokenImpersonationLevel.Impersonation);
                namedPipe.Connect((int)timeout.TotalMilliseconds);
                return namedPipe;
            }
            else
            {
                var socket = new UnixDomainSocket();
                socket.Connect(Path.Combine(IpcRootPath, address), timeout);
                return new ExposedSocketNetworkStream(socket, ownsSocket: true);
            }
        }

        public override async Task<Stream> ConnectAsync(CancellationToken token)
        {
            string address = GetDefaultAddress();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var namedPipe = new NamedPipeClientStream(
                    ".",
                    address,
                    PipeDirection.InOut,
                    PipeOptions.None,
                    TokenImpersonationLevel.Impersonation);
                await namedPipe.ConnectAsync(token).ConfigureAwait(false);
                return namedPipe;
            }
            else
            {
                var socket = new UnixDomainSocket();
                await socket.ConnectAsync(Path.Combine(IpcRootPath, address), token).ConfigureAwait(false);
                return new ExposedSocketNetworkStream(socket, ownsSocket: true);
            }
        }

        public override void WaitForConnection(TimeSpan timeout)
        {
            using var _ = Connect(timeout);
        }

        public override async Task WaitForConnectionAsync(CancellationToken token)
        {
            using var _ = await ConnectAsync(token).ConfigureAwait(false);
        }

        private string GetDefaultAddress()
        {
            try
            {
                var process = Process.GetProcessById(_pid);
            }
            catch (ArgumentException)
            {
                throw new ServerNotAvailableException($"Process {_pid} is not running.");
            }
            catch (InvalidOperationException)
            {
                throw new ServerNotAvailableException($"Process {_pid} seems to be elevated.");
            }

            if (!TryGetDefaultAddress(_pid, out string transportName))
            {
                throw new ServerNotAvailableException($"Process {_pid} not running compatible .NET runtime.");
            }

            return transportName;
        }

        private static bool TryGetDefaultAddress(int pid, out string defaultAddress)
        {
            defaultAddress = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                defaultAddress = $"dotnet-diagnostic-{pid}";
            }
            else
            {
                try
                {
                    defaultAddress = Directory.GetFiles(IpcRootPath, $"dotnet-diagnostic-{pid}-*-socket") // Try best match.
                        .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                        .FirstOrDefault();
                }
                catch (InvalidOperationException)
                {
                }
            }

            return !string.IsNullOrEmpty(defaultAddress);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PidIpcEndpoint);
        }

        public bool Equals(PidIpcEndpoint other)
        {
            return other != null && other._pid == _pid;
        }

        public override int GetHashCode()
        {
            return _pid.GetHashCode();
        }
    }
}