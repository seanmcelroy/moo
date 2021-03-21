using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Actions.BuiltIn;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;
using moo.common.Scripting;

namespace moo.common
{
    public class Server
    {
        private static Server Instance;

        private static readonly ConcurrentBag<PlayerConnection> _players = new();

        private static readonly List<IRunnable> globalActions = new()
        {
            new ActionBuiltIn(),
            new ChownBuiltIn(),
            new EditBuiltIn(),
            new LinkBuiltIn(),
            new LoadBuiltIn(),
            new NameBuiltIn(),
            new NewPasswordBuiltIn(),
            new RecycleBuiltIn(),
            new SaveBuiltIn(),
            new SetBuiltIn(),
            new ProgramBuiltIn(),
            new PropSetBuiltIn(),
            new Look(),
        };

        private static readonly ConcurrentBag<ForthProcess> processes = new();

        private static readonly ConcurrentQueue<ForthProcess> processesWaitingForPreempt = new();

        private readonly TextWriter statusWriter;

        private Task playerHandlerTask;

        public long PreemptProcessId;

        private Server(TextWriter statusWriter) => this.statusWriter = statusWriter;

        public static Server Initialize(TextWriter statusWriter)
        {
            if (Instance != null)
                throw new InvalidOperationException("Already initialized");
            Instance = new Server(statusWriter);
            return Instance;
        }

        public static Server GetInstance()
        {
            if (Instance == null)
                throw new InvalidOperationException("Not initialized");
            return Instance;
        }

        public static IEnumerable<PlayerConnection> GetConnectedPlayers()
        {
            foreach (var player in _players)
            {
                yield return player;
            }
        }

        public static bool IsPlayerConnected(Dbref dbref) => _players.Any(p => p.Dbref == dbref);

        public ConsoleConnection AttachConsolePlayer(HumanPlayer player, TextReader input, TextWriter output, CancellationToken cancellationToken)
        {
            var consoleConnection = new ConsoleConnection(player, input, output, cancellationToken);
            _players.Add(consoleConnection);
            statusWriter.WriteLine($"Console player {player.UnparseObjectInternal()} attached");
            return consoleConnection;
        }

        public static Script RegisterScript(string name, Player player, string? programText = null)
        {
            Script scriptObject = ThingRepository.Instance.Make<Script>();
            scriptObject.name = name ?? throw new System.ArgumentNullException(nameof(name));
            scriptObject.owner = player.id;
            scriptObject.programText = programText;
            if (player.HasFlag(Thing.Flag.WIZARD))
                scriptObject.SetFlag(Thing.Flag.WIZARD);
            else if (player.HasFlag(Thing.Flag.LEVEL_3))
                scriptObject.SetFlag(Thing.Flag.LEVEL_3);
            else if (player.HasFlag(Thing.Flag.LEVEL_2))
                scriptObject.SetFlag(Thing.Flag.LEVEL_2);
            else if (player.HasFlag(Thing.Flag.LEVEL_1))
                scriptObject.SetFlag(Thing.Flag.LEVEL_1);
            var insertedScriptObject = ThingRepository.Instance.Insert(scriptObject);
            globalActions.Add(insertedScriptObject);
            return insertedScriptObject;
        }

        public static async Task<ForthWordResult> ExecuteAsync(
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
                    (Interlocked.Read(ref PreemptProcessId) != 0 ||
                    !(processesWaitingForPreempt.TryPeek(out ForthProcess nextInLine) && nextInLine.ProcessId == processId)) &&
                    !processesWaitingForPreempt.TryDequeue(out ForthProcess dequeued))
                {
                    Thread.Yield();
                    Thread.Sleep(50);
                }

                await statusWriter.WriteLineAsync($"PID {target.ProcessId} is up, waiting to pre-empt other tasks");

                while (
                    !cancellationToken.IsCancellationRequested &&
                    processes.Any(p =>
                        p.State != ForthProcess.ProcessState.Paused &&
                        p.State != ForthProcess.ProcessState.Preempted &&
                        p.State != ForthProcess.ProcessState.Complete &&
                        p.ProcessId != processId))
                {
                    foreach (var p in processes.Where(p => p.State != ForthProcess.ProcessState.Paused && p.ProcessId != processId))
                    {
                        p.Pause();
                    }

                    Thread.Yield();
                    Thread.Sleep(100);
                }

                Interlocked.Exchange(ref PreemptProcessId, target.ProcessId);
                target.State = ForthProcess.ProcessState.RunningPreempt;
                await statusWriter.WriteLineAsync($"PID {target.ProcessId} in preempt mode");
            });

            Task.WaitAll(new[] { task }, 60 * 1000, cancellationToken);
        }

        public static PlayerConnection? GetConnection(Dbref playerId) => _players.Where(x => x.Dbref == playerId).FirstOrDefault();

        public static IEnumerable<int> GetConnectionNumber(Dbref playerId)
        {
            return _players.Select((conn, index) => new { conn, index }).Where(x => x.conn.Dbref == playerId).Select(x => x.index);
        }

        public static PlayerConnection? GetConnectionForConnectionNumber(int connecitonNumber)
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

        public static int? GetConnectionNumberForConnectionDescriptor(int connectionDescriptor) => _players
            .Select((conn, index) => new { conn, index })
            .Where(x => x.conn.ConnectorDescriptor == connectionDescriptor)
            .Select(x => x.index).SingleOrDefault();

        public static int GetConnectionCount(Dbref playerId) => _players.Count(p => p.Dbref == playerId);

        public static IEnumerable<int> GetConnectionDescriptors(Dbref playerId) => _players.Where(p => p.Dbref == playerId).Select(p => p.ConnectorDescriptor);

        public static IEnumerable<Tuple<Dbref, string>> GetConnectionPlayers() => _players.Select(conn => new Tuple<Dbref, string>(conn.Dbref, conn.Name));

        public static async Task<Player?> NotifyAsync(Dbref player, string message)
        {
            Player? ret = null;
            foreach (var connection in _players.Where(p => p.Dbref == player))
            {
                ret = connection.GetPlayer();
                await connection.SendOutput(message);
            }
            return ret;
        }

        public static async Task NotifyRoomAsync(Dbref room, string message, List<Dbref>? exclude = null)
        {
            foreach (var connection in _players.Where(p => p.Location == room && (exclude == null || !exclude.Contains(p.Dbref))))
                await connection.SendOutput(message);
        }

        public void Start(CancellationToken cancellationToken)
        {
            statusWriter.WriteLine("Initializing sqlite storage provider");
            var storageProvider = new SqliteStorageProvider();
            storageProvider.Initialize();
            ThingRepository.Instance.SetStorageProvider(storageProvider);

            var aether = ThingRepository.Instance.GetFromCacheOnly<Room>(Dbref.AETHER)!;
            var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            aether.SetPropertyPathValue("_sys/startuptime", new ForthVariable(now));

            playerHandlerTask = new Task(async () =>
            {
                do
                {
                    await Task.WhenAll(GetConnectedPlayers().Select(async (connection) => await connection.RunNextCommand(cancellationToken)));
                } while (!cancellationToken.IsCancellationRequested);

            }, cancellationToken);
            playerHandlerTask.Start();
        }

        public static async Task RunCommand(
                    Dbref player,
                    PlayerConnection? connection,
                    CommandResult command,
                    CancellationToken cancellationToken)
        {
            if (command == default || command.Raw.Length == 0)
                return;

            // Global actions
            VerbResult actionResult;
            foreach (var action in globalActions)
            {
                if (action.CanProcess(player, command).Item1)
                {
                    // TODO: Right now we block on programs
                    actionResult = await action.Process(player, connection, command, cancellationToken);
                    if (!actionResult.isSuccess && connection != null)
                        await connection.SendOutput($"ERROR: {actionResult.reason}");

                    return;
                }
            }

            var matchResult = await Matcher
                .InitObjectSearch(player, command.GetVerb(), Dbref.DbrefObjectType.Unknown, cancellationToken)
                .MatchEverything()
                .Result();

            // Todo, everything below here needs to be checked against Fuzzball
            if (matchResult.IsValid())
            {
                var matchedLookup = await ThingRepository.Instance.GetAsync(matchResult, cancellationToken);
                if (!matchedLookup.isSuccess || matchedLookup.value == null)
                {
                    if (connection != null)
                        await connection.SendOutput($"Cannot retrieve {matchResult}: {matchedLookup.reason}");
                    return;
                }

                if (matchResult.Type == Dbref.DbrefObjectType.Exit)
                {
                    var exit = (Exit)matchedLookup.value!;
                    if (!exit.CanProcess(player, command).Item1)
                    {
                        if (connection != null)
                            await connection.SendOutput($"Locked.");
                        return;
                    }

                    await exit.Process(player, connection, command, cancellationToken);
                    return;
                }

                if (connection != null)
                    await connection.SendOutput($"I don't know how to process {await matchedLookup.value.UnparseObject(player, cancellationToken)}");
            }

            if (command.Raw.StartsWith("@"))
                Console.WriteLine($"Unknown at-command: {command.Raw}");

            if (connection != null)
                await connection.SendOutput("Huh?");
        }
    }
}