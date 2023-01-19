using System;
using System.Net;
using moo.common.Connections;

namespace moo.common.Networking
{
/// <summary>
    /// Metadata about a connection from a client to the server instance
    /// </summary>
    public class ConnectionMetadata
    {
        /// <summary>
        /// Gets or sets the remote address of the client that is connected to the server
        /// </summary>
        public IPAddress RemoteAddress { get; set; }

        /// <summary>
        /// Gets or sets the remote port of the client that is connected to the server
        /// </summary>
        public int RemotePort { get; set; }

        /// <summary>
        /// Gets or sets the number of messages sent over this connection
        /// </summary>
        public ulong SentMessageCount { get;set;}

        /// <summary>
        /// Gets or sets the amount of data sent over this connection in bytes
        /// </summary>
        public ulong SentMessageBytes { get; set; }

        /// <summary>
        /// Gets or sets the number of messages received over this connection
        /// </summary>
        public ulong RecvMessageCount { get; set; }

        /// <summary>
        /// Gets or sets the amount of data received over this connection in bytes
        /// </summary>
        public ulong RecvMessageBytes { get; set; }

        /// <summary>
        /// Gets or sets the date the connection was opened, stored as a UTC value
        /// </summary>
        public DateTime Established { get; set; }

        /// <summary>
        /// Gets or sets the address that was listening for this connection when it was received, if this connection was an inbound address
        /// </summary>
        public IPAddress? ListenAddress { get; set; }

        /// <summary>
        /// Gets or sets the port that was listening for this connection when it was received, if this connection was an inbound address
        /// </summary>
        public int? ListenPort { get; set; }

        /// <summary>
        /// Gets or sets the connection this metadata is associated with
        /// </summary>
        public PlayerConnection Connection { get; internal set; }
    }
}