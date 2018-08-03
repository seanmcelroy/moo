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
    private static ConcurrentDictionary<Dbref, PlayerConnection> _players = new ConcurrentDictionary<Dbref, PlayerConnection>();

    private static readonly ConcurrentDictionary<Dbref, Action> actions = new ConcurrentDictionary<Dbref, Action>();

    private static readonly ConcurrentBag<ForthProcess> processes = new ConcurrentBag<ForthProcess>();

    private readonly TextWriter statusWriter;

    private Task playerHandlerTask;

    public Server(TextWriter statusWriter)
    {
        this.statusWriter = statusWriter;
    }

    public static IEnumerable<PlayerConnection> GetConnectedPlayers()
    {
        foreach (var player in _players.Values)
        {
            yield return player;
        }
    }

    public void AttachConsolePlayer(HumanPlayer player, TextReader input, TextWriter output)
    {
        var consoleConnection = new ConsoleConnection(player, input, output);

        if (_players.TryAdd(player.id, consoleConnection))
        {
            statusWriter.WriteLine($"Console player {player.name}({player.id}) attached");
        }
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

    public async Task<ForthProgramResult> ExecuteAsync(
        ForthProcess process,
        Dbref trigger,
        string command,
        object[] args,
        CancellationToken cancellationToken)
    {
        processes.Add(process);
        var programResult = await process.RunAsync(trigger, command, args, cancellationToken);
        return programResult;
    }

    public int GetConnectionCount(Dbref playerId)
    {
        return _players.Count(p => p.Key == playerId);
    }

    public IEnumerable<int> GetConnectionDescriptors(Dbref playerId)
    {
        return _players.Where(p => p.Key == playerId).Select(p => p.Value.ConnectorDescriptor);
    }

    public void Notify(Dbref target, string message)
    {
        PlayerConnection targetConnection;
        if (_players.TryGetValue(target, out targetConnection))
            targetConnection.sendOutput(message);
    }

    public void Start(CancellationToken cancellationToken)
    {

        statusWriter.WriteLine("Initializing sqlite storage provider");
        var storageProvider = new SqliteStorageProvider();
        storageProvider.Initialize();
        ThingRepository.setStorageProvider(storageProvider);

        var aether = ThingRepository.GetFromCacheOnly<Room>(new Dbref(0, DbrefObjectType.Room));
        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

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