using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Models;

namespace moo.common.Scripting
{
    public struct ForthPrimativeParameters
    {
        private Dbref? lastListItem;

        public readonly ForthProcess? Process;

        public readonly Stack<ForthDatum> Stack;

        private readonly Dictionary<string, ForthVariable>? variables;

        public readonly PlayerConnection? Connection;

        public readonly Dbref Trigger;

        public readonly string? Command;

        public readonly CancellationToken CancellationToken;

        public Dbref? LastListItem => lastListItem;

        public ImmutableDictionary<string, ForthVariable>? Variables => variables?.ToImmutableDictionary();

        public ForthPrimativeParameters(
            ForthProcess? process,
            Stack<ForthDatum> stack,
            Dictionary<string, ForthVariable>? variables,
            PlayerConnection? connection,
            Dbref trigger,
            string? command,
            Func<Dbref, string, Task>? notify,
            Func<Dbref, string, List<Dbref>, Task>? notifyRoom,
            Dbref? lastListItem,
            CancellationToken cancellationToken)
        {
            Process = process;
            Stack = stack;
            this.variables = variables;
            Connection = connection;
            Trigger = trigger;
            Command = command;
            this.lastListItem = lastListItem;
            CancellationToken = cancellationToken;
        }
    }
}