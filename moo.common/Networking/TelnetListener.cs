using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace moo.common.Networking
{
    internal class TelnetListener : TcpListener
    {
        // Thread signal.
        private readonly TelnetServer server;

        public TelnetListener(TelnetServer server, IPEndPoint localEp)
            : base(localEp)
        {
            this.server = server;
        }

        public async void StartAccepting()
        {
            // Establish the local endpoint for the socket.
            var localEndPoint = new IPEndPoint(IPAddress.Any, ((IPEndPoint)this.LocalEndpoint).Port);

            // Create a TCP/IP socket.
            var listener = new TelnetListener(this.server, localEndPoint);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Start(100);

                while (true)
                {
                    // Start an asynchronous socket to listen for connections.
                    var handler = await listener.AcceptTcpClientAsync();

                    // Create the state object.
                    var stream = handler.GetStream();
                    var telnetConnection = new TelnetConnection(server, handler, stream);
                    this.server.AddConnection(telnetConnection);

                    telnetConnection.Process();
                }

            }
            catch (Exception ex)
            {
                if (server.Logger == null)
                    throw;
                server.Logger.LogError(ex, "Exception when trying to accept connection from listener");
            }
        }
    }
}