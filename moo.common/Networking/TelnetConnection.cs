using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using moo.common.Connections;

namespace moo.common.Networking
{
    public class TelnetConnection : PlayerConnection
    {
        /// <summary>
        /// The size of the stream receive buffer
        /// </summary>
        private const int BufferSize = 1024;

        private readonly TelnetServer server;

        /// <summary>
        /// The <see cref="TcpClient"/> that accepted this connection.
        /// </summary>
        private readonly TcpClient client;

        /// <summary>
        /// The <see cref="Stream"/> instance retrieved from the <see cref="TcpClient"/> that accepted this connection.
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// The stream receive buffer
        /// </summary>
        private readonly byte[] buffer = new byte[BufferSize];

        /// <summary>
        /// The received data buffer appended to from the stream buffer
        /// </summary>
        private readonly StringBuilder builder = new();

        /// <summary>
        /// The remote IP address to which the connection is established
        /// </summary>
        private readonly IPAddress remoteAddress;

        /// <summary>
        /// The remote TCP port number for the remote endpoint to which the connection is established
        /// </summary>
        private readonly int remotePort;

        /// <summary>
        /// The local IP address to which the connection is established
        /// </summary>
        private readonly IPAddress localAddress;

        /// <summary>
        /// The local TCP port number for the local endpoint to which the connection is established
        /// </summary>
        private readonly int localPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelnetConnection"/> class.
        /// </summary>
        /// <param name="server">The server instance that owns this connection</param>
        /// <param name="client">The <see cref="TcpClient"/> that accepted this connection</param>
        /// <param name="stream">The <see cref="Stream"/> from the <paramref name="client"/></param>
        public TelnetConnection(
             TelnetServer server,
             TcpClient client,
             Stream stream) : base(null)
        {
            this.client = client;
            this.client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            this.server = server;
            this.stream = stream;

            var remoteIpEndpoint = (IPEndPoint)this.client.Client.RemoteEndPoint;
            this.remoteAddress = remoteIpEndpoint.Address;
            this.remotePort = remoteIpEndpoint.Port;
            var localIpEndpoint = (IPEndPoint)this.client.Client.LocalEndPoint;
            this.localAddress = localIpEndpoint.Address;
            this.localPort = localIpEndpoint.Port;
        }

        public bool ShowBytes { get; set; }

        public bool ShowCommands { get; set; }

        public bool ShowData { get; set; }

        public string PathHost { get; set; }

        #region Derived instance properties
        /// <summary>
        /// Gets the remote IP address to which the connection is established
        /// </summary>
        public IPAddress RemoteAddress => this.remoteAddress;

        /// <summary>
        /// Gets the remote TCP port number for the remote endpoint to which the connection is established
        /// </summary>
        public int RemotePort => this.remotePort;

        /// <summary>
        /// Gets the local IP address to which the connection is established
        /// </summary>
        public IPAddress LocalAddress => this.localAddress;

        /// <summary>
        /// Gets the local TCP port number for the local endpoint to which the connection is established
        /// </summary>
        public int LocalPort => this.localPort;
        #endregion

        #region IO and Connection Management
        public async void Process()
        {
            try
            {
                while (true)
                {
                    if (!this.client.Connected || !this.client.Client.Connected) return;

                    if (!this.stream.CanRead)
                    {
                        await this.Shutdown();
                        return;
                    }

                    var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, BufferSize));

                    // There  might be more data, so store the data received so far.
                    builder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));

                    // Not all data received OR no more but not yet ending with the delimiter. Get more.
                    var content = this.builder.ToString();
                    if (bytesRead == BufferSize || !content.EndsWith("\r\n", StringComparison.Ordinal))
                    {
                        // Read some more.
                        continue;
                    }

                    ReceiveInput(content.TrimEnd('\r', '\n'));
                    builder.Clear();
                }
            }
            catch (DecoderFallbackException dfe)
            {
                if (server.Logger == null)
                    throw;
                server.Logger.LogError(dfe, "Decoder Fallback Exception socket {RemoteAddress}", RemoteAddress);
            }
            catch (IOException se)
            {
                if (server.Logger == null)
                    throw;
                server.Logger.LogError(se, "I/O Exception on socket {RemoteAddress}", RemoteAddress);
            }
            catch (SocketException se)
            {
                if (server.Logger == null)
                    throw;
                server.Logger.LogError(se, "Socket Exception on socket {RemoteAddress}", RemoteAddress);
            }
            catch (NotSupportedException nse)
            {
                if (server.Logger == null)
                    throw;
                server.Logger.LogError(nse, "Not Supported Exception");
                return;
            }
            catch (ObjectDisposedException ode)
            {
                if (server.Logger == null)
                    throw;
                server.Logger.LogError(ode, "Object Disposed Exception");
                return;
            }
        }

        /// <summary>
        /// Sends the formatted data to the client
        /// </summary>
        /// <param name="format">The data, or format string for data, to send to the client</param>
        /// <param name="args">The argument applied as a format string to <paramref name="format"/> to create the data to send to the client</param>
        /// <returns>A value indicating whether or not the transmission was successful</returns>
        private async Task<bool> Send(string format, params object[] args)
        {
            return await SendOutput(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        public async override Task<bool> SendOutput(string output)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(output);

            try
            {
                // Begin sending the data to the remote device.
                await stream.WriteAsync(byteData);
                if (this.ShowBytes && this.ShowData)
                    server.Logger?.LogTrace(
                       "{RemoteAddress}:{RemotePort} <<< {byteDataLength} bytes: {data}", RemoteAddress, RemotePort, byteData.Length, output.TrimEnd('\r', '\n'));
                else if (this.ShowBytes)
                    server.Logger?.LogTrace(
                        "{RemoteAddress}:{RemotePort} <<< {byteDataLength} bytes", RemoteAddress, RemotePort, byteData.Length);
                else if (this.ShowData)
                    server.Logger?.LogTrace(
                         "{RemoteAddress}:{RemotePort} <<< {data}", RemoteAddress, RemotePort, output.TrimEnd('\r', '\n'));

                return true;
            }
            catch (IOException)
            {
                // Don't send a response - the sending socket isn't working.
                server.Logger?.LogDebug("{RemoteAddress}:{RemotePort} XXX CONNECTION TERMINATED", RemoteAddress, RemotePort);
                return false;
            }
            catch (SocketException)
            {
                // Don't send a response - the sending socket isn't working.
                server.Logger?.LogDebug("{RemoteAddress}:{RemotePort} XXX CONNECTION TERMINATED", RemoteAddress, RemotePort);
                return false;
            }
            catch (ObjectDisposedException)
            {
                // Don't send a response - the sending socket isn't working.
                server.Logger?.LogDebug("{RemoteAddress}:{RemotePort} XXX CONNECTION TERMINATED", RemoteAddress, RemotePort);
                return false;
            }
        }

        public async Task Shutdown()
        {
            if (client.Connected)
            {
                await Send("205 closing connection\r\n");
                client.Client.Shutdown(SocketShutdown.Both);
                client.Close();
            }

            server.RemoveConnection(this);
        }
        #endregion
    }
}