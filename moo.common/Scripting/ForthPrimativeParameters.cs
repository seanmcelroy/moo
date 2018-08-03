using System;
using System.Collections.Generic;
using System.Threading;

public struct ForthPrimativeParameters
{
    public readonly Server Server;
    public readonly Stack<ForthDatum> Stack;
    public readonly Dictionary<string, object> Variables;

    public readonly PlayerConnection Connection;

    public readonly Dbref Trigger;

    public readonly string Command;

    public readonly Action<Dbref, string> Notify;

    public readonly CancellationToken CancellationToken;

    public ForthPrimativeParameters(Server server, Stack<ForthDatum> stack, Dictionary<string, object> variables, PlayerConnection connection, Dbref trigger, string command, Action<Dbref, string> notify, CancellationToken cancellationToken)
    {
        this.Server = server;
        this.Stack = stack;
        this.Variables = variables;
        this.Connection = connection;
        this.Trigger = trigger;
        this.Command = command;
        this.Notify = notify;
        this.CancellationToken = cancellationToken;
    }
}