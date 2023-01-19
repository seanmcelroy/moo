using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace moo.common.Networking
{
    public class TelnetServer
    {
        /// <summary>
        /// A list of threads and the associated TCP new-connection listeners that are serviced by each by the client
        /// </summary>
        private readonly List<Tuple<Thread, TelnetListener>> listeners = new();

        /// <summary>
        /// A list of connections currently established to this server instance
        /// </summary>
        private readonly List<TelnetConnection> connections = new();

        internal ILogger? Logger { private set; get; }

        public ReadOnlyCollection<ConnectionMetadata> Connections => connections.Select(c => new ConnectionMetadata
        {
            RemoteAddress = c.RemoteAddress,
            RemotePort = c.RemotePort
        })
              .ToList()
              .AsReadOnly();


        public int[] TelnetClearPorts { get; set; }

        TelnetServer(ILogger? logger)
        {
            Logger = logger;
        }

        #region Connection and IO
        /// <summary>
        /// Starts listener threads to begin processing requests
        /// </summary>
        public void Start()
        {
            this.listeners.Clear();

            foreach (var clearPort in this.TelnetClearPorts)
            {
                // Establish the local endpoint for the socket.
                var localEndPoint = new IPEndPoint(IPAddress.Any, clearPort);

                // Create a TCP/IP socket.
                var listener = new TelnetListener(this, localEndPoint);

                this.listeners.Add(new Tuple<Thread, TelnetListener>(new Thread(listener.StartAccepting), listener));
            }

            foreach (var listener in this.listeners)
            {
                try
                {
                    listener.Item1.Start();
                    Logger?.LogInformation("Listening on port {Port}", ((IPEndPoint)listener.Item2.LocalEndpoint).Port);
                }
                catch (OutOfMemoryException oom)
                {
                    if (Logger == null)
                        throw;
                    Logger?.LogError(oom, "Unable to start listener thread.  Not enough memory.");
                }
            }
        }

        public void Stop()
        {
            foreach (var listener in this.listeners)
            {
                try
                {
                    listener.Item2.Stop();
                    var le = (IPEndPoint)listener.Item2.LocalEndpoint;
                    Logger?.LogInformation("Stopped listening on port {Address}:{Port}", le.Address, le.Port);
                }
                catch (SocketException se)
                {
                    if (Logger == null)
                        throw;
                    Logger.LogError(se, "Exception attempting to stop listening on port {Port}", ((IPEndPoint)listener.Item2.LocalEndpoint).Port);
                }
            }

            Task.WaitAll(this.connections.Select(connection => connection.Shutdown()).ToArray());

            foreach (var thread in this.listeners)
            {
                try
                {
                    thread.Item1.Abort();
                }
                catch (SecurityException se)
                {
                    Logger?.LogError(se, "Unable to abort the thread due to a security exception.  Application will now exit.");
                    Environment.Exit(se.HResult);
                }
                catch (ThreadStateException tse)
                {
                    Logger?.LogError(tse, "Unable to abort the thread due to a thread state exception.  Application will now exit.");
                    Environment.Exit(tse.HResult);
                }
            }
        }

        internal void AddConnection(TelnetConnection telnetConnection)
        {
            connections.Add(telnetConnection);
            Logger?.LogDebug("Connection from {RemoteAddress}:{RemotePort} to {LocalAddress}:{LocalPort}", telnetConnection.RemoteAddress, telnetConnection.RemotePort, telnetConnection.LocalAddress, telnetConnection.LocalPort);
        }

        internal void RemoveConnection(TelnetConnection telnetConnection)
        {
            connections.Remove(telnetConnection);
            if (telnetConnection.Name == null)
                Logger?.LogInformation("Disconnection from {RemoteAddress}:{RemotePort}", telnetConnection.RemoteAddress, telnetConnection.RemotePort);
            else
                Logger?.LogInformation("Disconnection from {RemoteAddress}:{RemotePort} ({Name})", telnetConnection.RemoteAddress, telnetConnection.RemotePort, telnetConnection.Name);
        }
        #endregion
    }
}