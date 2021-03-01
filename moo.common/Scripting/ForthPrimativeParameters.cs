using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public struct ForthPrimativeParameters
{
    private Dbref? lastListItem;

    public readonly ForthProcess? Process;

    public readonly Stack<ForthDatum> Stack;

    public readonly Dictionary<string, ForthVariable>? Variables;

    public readonly PlayerConnection? Connection;

    public readonly Dbref Trigger;

    public readonly string? Command;

    public readonly CancellationToken CancellationToken;

    public Dbref? LastListItem => lastListItem;

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
        this.Process = process;
        this.Stack = stack;
        this.Variables = variables;
        this.Connection = connection;
        this.Trigger = trigger;
        this.Command = command;
        this.lastListItem = lastListItem;
        this.CancellationToken = cancellationToken;
    }
}