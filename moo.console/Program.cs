using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace moo.console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\r\n\r\nMoo!\r\n");
            CancellationTokenSource cts = new CancellationTokenSource();

            Console.Out.WriteLine("Initializing sqlite storage provider");
            SqliteStorageProvider storageProvider = new SqliteStorageProvider();
            storageProvider.Initialize();
            ThingRepository.setStorageProvider(storageProvider);

            Console.Out.WriteLine("Loading initial pillars");
            Player consolePlayer = LoadSandbox();

            var aether = ThingRepository.GetFromCacheOnly<Room>((Dbref)0);
            var now = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
            aether.SetPropertyPathValue("_sys/startuptime", new ForthDatum(now));
            var nowRead = aether.GetPropertyPathValue("_sys/startuptime");


            Console.Out.WriteLine("Loading built-in actions");
            LoadBuiltInActions();

            Task consoleTask = Task.Factory.StartNew(async () =>
            {
                await Console.Out.WriteLineAsync("Starting console interface");
                do
                {
                    String input = await Console.In.ReadLineAsync();
                    consolePlayer.receiveInput(input + "\r\n");
                } while (true);
            });

            do
            {
                Parallel.ForEach(Player.all(), async (player) =>
                {
                    CommandResult command = await player.popCommand();
                    if (default(CommandResult).Equals(command))
                        return;

                    await Console.Out.WriteLineAsync($"{player.name}({player.id}): {command.raw}");

                    var result = await CommandHandler.HandleHumanCommandAsync(player, command, cts.Token);
                    if (!result.isSuccess)
                    {
                        await player.sendOutput("ERROR: " + result.reason);
                    }
                });

            } while (!cts.IsCancellationRequested);
        }

        static void LoadBuiltInActions()
        {

            foreach (var actionType in new Action[] { new @Load(), new Look(), new @Save() })
            {
                Action actionObject = ThingRepository.Insert(actionType);
                CommandHandler.actions.TryAdd(actionObject.id, actionObject);
            }
        }

        private static Player LoadSandbox()
        {
            Room aether = Room.Make("The Aether");
            aether.internalDescription = "You see a massless, infinite black void stretching forever in all directions and across all time.";
            HostPlayer.make("God", aether);
            var player = ConsolePlayer.make("Intrepid Hero", aether);

            Script.Make("foo", "\"foo\" \"bar\" \"baz\" 2 rotate 2 rotate 3 rotate -2 rotate 3 pick POP POP POP POP");

            Script.Make("test-rotate1", "\"a\" \"b\" \"c\" \"d\" 4 rotate");
            Script.Make("test-rotate2", "\"a\" \"b\" \"c\" \"d\" -4 rotate");
            Script.Make("test-put", "\"a\" \"b\" \"c\" \"d\" \"e\" 3 put");
            Script.Make("test-reverse", "\"a\" \"b\" \"c\" \"d\" \"e\" 4 reverse");
            Script.Make("test-lreverse", "\"a\" \"b\" \"c\" \"d\" \"e\" 4 lreverse");
            Script.Make("test-marker", "{ \"a\" \"b\" \"c\" }");
            Script.Make("test-@", "me @ loc @ trigger @ command @ POP POP POP POP");
            Script.Make("test-vars", "LVAR test\r\n1234 test !\r\ntest @");
            Script.Make("test-math", "3 5 1.1 2 2 1.01 9 2 1 2 3 + + * / - * > + INT + %");

            Script.Make("cmd-uptime", LoadScriptFile("scripts/cmd-upload.muf"));

            return player;
        }

        private static string LoadScriptFile(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
    }
}