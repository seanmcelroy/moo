using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;
using moo.common.Scripting;

namespace moo.common
{
    public class Server
    {
        private static Server Instance;

        private static readonly ConcurrentQueue<PlayerConnection> _players = new();

        private static readonly ConcurrentBag<IRunnable> globalActions = new();

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
            _players.Enqueue(consoleConnection);
            statusWriter.WriteLine($"Console player {player.UnparseObject()} attached");
            return consoleConnection;
        }

        public static void RegisterBuiltInAction(IRunnable action)
        {
            if (action == null)
                throw new System.ArgumentNullException(nameof(action));

            globalActions.Add(action);
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

        public static async Task NotifyAsync(Dbref player, string message)
        {
            foreach (var connection in _players.Where(p => p.Dbref == player))
                await connection.sendOutput(message);
        }

        public static async Task NotifyRoomAsync(Dbref room, string message, List<Dbref>? exclude = null)
        {
            foreach (var connection in _players.Where(p => p.Location == room && (exclude == null || !exclude.Contains(p.Dbref))))
                await connection.sendOutput(message);
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
                    await Task.WhenAll(GetConnectedPlayers().Select(async (connection) => await connection.RunNextCommand(globalActions, cancellationToken)));
                } while (!cancellationToken.IsCancellationRequested);

            }, cancellationToken);
            playerHandlerTask.Start();
        }
    }
}