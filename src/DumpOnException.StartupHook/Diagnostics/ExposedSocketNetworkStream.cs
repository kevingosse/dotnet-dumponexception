using System.Net.Sockets;

namespace DumpOnException.StartupHook.Diagnostics
{
    internal sealed class ExposedSocketNetworkStream :
        NetworkStream
    {
        public ExposedSocketNetworkStream(Socket socket, bool ownsSocket)
            : base(socket, ownsSocket)
        {
        }

        public new Socket Socket => base.Socket;
    }
}