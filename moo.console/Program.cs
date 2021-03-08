using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using moo.common;
using moo.common.Actions.BuiltIn;
using moo.common.Connections;
using moo.common.Models;

namespace moo.console
{
    class Program
    {
        static void Main()
        {
            Console.Out.WriteLine("\r\n\r\nMoo!\r\n");
            var cts = new CancellationTokenSource();

            try
            {
                var server = Server.Initialize(Console.Out);

                var consolePlayer = LoadSandbox(cts.Token);
                var consoleConnection = server.AttachConsolePlayer(consolePlayer, Console.In, Console.Out, cts.Token);

                Console.Out.WriteLine("\r\nLoading built-in actions");
                foreach (var action in new IRunnable[] {
                new ActionBuiltIn(),
                new ChownBuiltIn(),
                new EditBuiltIn(),
                new LinkBuiltIn(),
                new LoadBuiltIn(),
                new NameBuiltIn(),
                new RecycleBuiltIn(),
                new SaveBuiltIn(),
                new SetBuiltIn(),
                new ProgramBuiltIn(),
                new PropSetBuiltIn() })
                    Server.RegisterBuiltInAction(action);

                Console.Out.WriteLine("Starting server");
                server.Start(cts.Token);

                Console.Out.WriteLine("Loading script directory");
                var scriptsToLoadInOrder = new string[] {
                $"scripts{System.IO.Path.DirectorySeparatorChar}cmd-@register.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}std-defs.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-strings.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-stackrng.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-props.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-lmgr.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-edit.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-gui.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-editor.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-match.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-mesg.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-mesgbox.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-look.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-reflist.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-index.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-case.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-mpi.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-arrays.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-bolding.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-debug.muf",
                //$"scripts{System.IO.Path.DirectorySeparatorChar}lib-mail-MOSS1.1.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-optionsinfo.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-optionsmisc.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-optionsmenu.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}lib-optionsgui.muf",
                $"scripts{System.IO.Path.DirectorySeparatorChar}cmd-@archive.muf",
            };
                var scriptsToLoad = scriptsToLoadInOrder
                    .Union(System.IO.Directory.GetFiles("scripts", "*.muf")
                    .OrderBy(f => f)
                    .Except(scriptsToLoadInOrder))
                    .Distinct();

                foreach (var scriptPath in scriptsToLoad)
                {
                    Task.WaitAll(Task.Run(async () =>
                    {
                        await ReadInScriptFile(scriptPath, consoleConnection);
                        while (!consoleConnection.IsIdle)
                        {
                            Thread.Sleep(250);
                        }
                        await consoleConnection.sendOutput($"Read in script file {scriptPath}");
                    }));
                }

                while (!cts.IsCancellationRequested)
                {

                    /*var input = System.Console.ReadLine();
                    if (string.Compare("QUIT", input) == 0)
                    {
                        cts.Cancel();
                    }*/
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static HumanPlayer LoadSandbox(CancellationToken cancellationToken)
        {
            var aether = Room.Make("The Aether", Dbref.GOD);
            aether.id = Dbref.AETHER;
            HostPlayer.make("God", aether);
            var player = HumanPlayer.make("Wizard", aether);
            player.SetFlag(Thing.Flag.WIZARD);

            Console.Out.WriteLine($"Created new player {player.UnparseObject()}");

            var moveResult = player.MoveToAsync(aether, cancellationToken).Result;

            /*/
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
            Script.Make("test-rinstr", ":test \"abcbcba\" \"bc\" rinstr ;");
            */

            return player;
        }

        private static void LoadScriptDirectory(string scriptDirectoryPath, string scriptSearchPattern, string? scriptFilePrefix = null)
        {
            foreach (var file in System.IO.Directory.GetFiles(scriptDirectoryPath, scriptSearchPattern))
            {
                var commandName = file[(scriptDirectoryPath.Length + 1)..];
                if (scriptFilePrefix != null)
                    commandName = commandName.Replace(scriptFilePrefix, "");
                commandName = commandName.Replace(".muf", "");
                var script = Server.RegisterScript(commandName, Dbref.GOD, LoadScriptFile(file));
                Console.Out.WriteLine($"Created new script {script.UnparseObject()}");
            }
        }


        private static string LoadScriptFile(string path) => System.IO.File.ReadAllText(path);

        private static async Task ReadInScriptDirectory(PlayerConnection connection, string scriptDirectoryPath, string scriptSearchPattern, string? scriptFilePrefix = null)
        {
            foreach (var file in System.IO.Directory.GetFiles(scriptDirectoryPath, scriptSearchPattern).OrderBy(f => f))
            {
                await ReadInScriptFile(file, connection);
                await connection.sendOutput($"Read in script file {file}");
            }
        }

        private static async Task ReadInScriptFile(string path, PlayerConnection connection)
        {
            var lines = await System.IO.File.ReadAllLinesAsync(path);
            connection.ReceiveInputUnattended(lines);
        }
    }
}