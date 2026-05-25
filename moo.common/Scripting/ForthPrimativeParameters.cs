using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using moo.common.Models;

namespace moo.common.Scripting
{
    public readonly struct ForthPrimativeParameters
    {
        private readonly Dbref? lastListItem;

        public readonly ForthProcess? Process;

        public readonly Stack<ForthDatum> Stack;

        private readonly Dictionary<string, ForthVariable>? variables;

        public readonly Dbref Player;

        public readonly Dbref Location;

        public readonly Dbref Trigger;

        public readonly string? Command;

        public readonly ILogger? logger;

        public readonly CancellationToken CancellationToken;

        public readonly Dbref? LastListItem => lastListItem;

        public readonly IReadOnlyDictionary<string, ForthVariable>? Variables => variables;

        public readonly Func<Dbref, string, Task>? Notify;
        public readonly Func<Dbref, string, List<Dbref>, Task>? NotifyRoom;

        public ForthPrimativeParameters(
            ForthProcess? process,
            Stack<ForthDatum> stack,
            Dictionary<string, ForthVariable>? variables,
            Dbref player,
            Dbref location,
            Dbref trigger,
            string? command,
            Func<Dbref, string, Task>? notify,
            Func<Dbref, string, List<Dbref>, Task>? notifyRoom,
            Dbref? lastListItem,
            ILogger? logger,
            CancellationToken cancellationToken)
        {
            Process = process;
            Stack = stack;
            this.variables = variables;
            Player = player;
            Location = location;
            Trigger = trigger;
            Command = command;
            Notify = notify;
            NotifyRoom = notifyRoom;
            this.lastListItem = lastListItem;
            this.logger = logger;
            CancellationToken = cancellationToken;
        }
    }
}