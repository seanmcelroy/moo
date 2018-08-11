using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;

public class Server
{
    private static ConcurrentQueue<PlayerConnection> _players = new ConcurrentQueue<PlayerConnection>();

    private static readonly ConcurrentDictionary<Dbref, Action> actions = new ConcurrentDictionary<Dbref, Action>();

    private static readonly ConcurrentBag<ForthProcess> processes = new ConcurrentBag<ForthProcess>();

    private static readonly ConcurrentQueue<ForthProcess> processesWaitingForPreempt = new ConcurrentQueue<ForthProcess>();

    private readonly TextWriter statusWriter;

    private Task playerHandlerTask;

    private long preemptProcessId;

    public Server(TextWriter statusWriter)
    {
        this.statusWriter = statusWriter;
    }

    public static IEnumerable<PlayerConnection> GetConnectedPlayers()
    {
        foreach (var player in _players)
        {
            yield return player;
        }
    }

    public void AttachConsolePlayer(HumanPlayer player, TextReader input, TextWriter output)
    {
        var consoleConnection = new ConsoleConnection(player, input, output);
        _players.Enqueue(consoleConnection);
        statusWriter.WriteLine($"Console player {player.name}({player.id}) attached");
    }

    public void RegisterBuiltInAction(Action action)
    {
        if (action == null)
            throw new System.ArgumentNullException(nameof(action));

        var actionObject = ThingRepository.Insert(action);
        actions.TryAdd(actionObject.id, actionObject);
    }

    public Script RegisterScript(string name, string programText)
    {
        if (name == null)
            throw new System.ArgumentNullException(nameof(name));
        if (programText == null)
            throw new System.ArgumentNullException(nameof(programText));

        var scriptObject = ThingRepository.Make<Script>();
        scriptObject.name = name;
        scriptObject.programText = programText;
        var insertedScriptObject = ThingRepository.Insert(scriptObject);
        actions.TryAdd(insertedScriptObject.id, insertedScriptObject);
        return insertedScriptObject;
    }

    public async Task<ForthWordResult> ExecuteAsync(
        ForthProcess process,
        IEnumerable<ForthWord> words,
        Dbref trigger,
        string command,
        object[] args,
        CancellationToken cancellationToken)
    {
        processes.Add(process);
        var programResult = await process.RunAsync(words, trigger, command, args, cancellationToken);
        return programResult;
    }

    public async Task PreemptProcess(int processId, CancellationToken cancellationToken)
    {
        var target = processes.SingleOrDefault(p => p.ProcessId == processId);
        if (target == null)
            return;

        processesWaitingForPreempt.Enqueue(target);
        await statusWriter.WriteLineAsync($"PID {target.ProcessId} queueing for preempt mode");

        var task = Task.Factory.StartNew(async () =>
        {
            while (!cancellationToken.IsCancellationRequested &&
                (Interlocked.Read(ref preemptProcessId) != 0 ||
                !(processesWaitingForPreempt.TryPeek(out ForthProcess nextInLine) && nextInLine.ProcessId == processId)) &&
                !processesWaitingForPreempt.TryDequeue(out ForthProcess dequeued))
            {
                Thread.Yield();
                Thread.Sleep(50);
            }

            await statusWriter.WriteLineAsync($"PID {target.ProcessId} is up, waiting to pre-empt other tasks");

            while (
                !cancellationToken.IsCancellationRequested &&
                processes.Any(p => p.State != ForthProcess.ProcessState.Paused && p.ProcessId != processId))
            {
                foreach (var p in processes.Where(p => p.State != ForthProcess.ProcessState.Paused && p.ProcessId != processId))
                {
                    p.Pause();
                }

                Thread.Yield();
                Thread.Sleep(100);
            }

            Interlocked.Exchange(ref preemptProcessId, target.ProcessId);
            target.State = ForthProcess.ProcessState.RunningPreempt;
            await statusWriter.WriteLineAsync($"PID {target.ProcessId} in preempt mode");
        });

        Task.WaitAll(new[] { task }, 60 * 1000, cancellationToken);
    }

    public IEnumerable<int> GetConnectionNumber(Dbref playerId)
    {
        return _players.Select((conn, index) => new { conn, index }).Where(x => x.conn.Dbref == playerId).Select(x => x.index);
    }


    public PlayerConnection GetConnectionForConnectionNumber(int connecitonNumber)
    {
        try
        {
            return _players.ToArray()[connecitonNumber];
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }

    public int? GetConnectionNumberForConnectionDescriptor(int connectionDescriptor)
    {
        return _players.Select((conn, index) => new { conn, index }).Where(x => x.conn.ConnectorDescriptor == connectionDescriptor).Select(x => x.index).SingleOrDefault();
    }

    public int GetConnectionCount(Dbref playerId)
    {
        return _players.Count(p => p.Dbref == playerId);
    }

    public IEnumerable<int> GetConnectionDescriptors(Dbref playerId)
    {
        return _players.Where(p => p.Dbref == playerId).Select(p => p.ConnectorDescriptor);
    }

    public async Task NotifyAsync(Dbref player, string message)
    {
        foreach (var connection in _players.Where(p => p.Dbref == player))
            await connection.sendOutput(message);
    }

    public async Task NotifyRoomAsync(Dbref room, string message, List<Dbref> exclude = null)
    {
        foreach (var connection in _players.Where(p => p.Location == room && (exclude == null || !exclude.Contains(p.Dbref))))
            await connection.sendOutput(message);
    }

    public void Start(CancellationToken cancellationToken)
    {

        statusWriter.WriteLine("Initializing sqlite storage provider");
        var storageProvider = new SqliteStorageProvider();
        storageProvider.Initialize();
        ThingRepository.setStorageProvider(storageProvider);

        var aether = ThingRepository.GetFromCacheOnly<Room>(new Dbref(0, DbrefObjectType.Room));
        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        aether.SetPropertyPathValue("_sys/startuptime", new ForthVariable(now));

        playerHandlerTask = new Task(() =>
        {
            do
            {
                Parallel.ForEach(GetConnectedPlayers(), async (connection) =>
                {
                    var command = await connection.popCommand();
                    if (default(CommandResult).Equals(command))
                        return;

                    await statusWriter.WriteLineAsync($"{connection.Name}({connection.Dbref}): {command.raw}");

                    VerbResult actionResult;
                    foreach (var action in actions.Values)
                    {
                        if (action.CanProcess(connection, command).Item1)
                        {
                            // TODO: Right now we block on programs
                            actionResult = await action.Process(this, connection, command, cancellationToken);
                            if (!actionResult.isSuccess)
                            {
                                await connection.sendOutput("ERROR: " + actionResult.reason);
                                return;
                            }

                            return;
                        }
                    }

                    await connection.sendOutput("Huh?");
                    actionResult = new VerbResult(false, "Command not found for verb " + command.getVerb());
                });

            } while (!cancellationToken.IsCancellationRequested);

        }, cancellationToken);
        playerHandlerTask.Start();
    }
}