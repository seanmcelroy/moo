using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;

namespace moo.console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Out.WriteLine("\r\n\r\nMoo!\r\n");
            var cts = new CancellationTokenSource();

            var server = Server.Initialize(Console.Out);

            var consolePlayer = LoadSandbox(cts.Token);
            var consoleConnection = server.AttachConsolePlayer(consolePlayer, Console.In, Console.Out, cts.Token);

            Console.Out.WriteLine("Loading built-in actions");
            foreach (var action in new IRunnable[] {
                new ActionBuiltIn(),
                new LinkBuiltIn(),
                new LoadBuiltIn(),
                new SaveBuiltIn(),
                new ProgramBuiltIn() })
                server.RegisterBuiltInAction(action);

            Console.Out.WriteLine("Loading script directory");
            ReadInScriptDirectory(consoleConnection, "scripts", "cmd-@register.muf", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "std-defs.muf", null);
            /*ReadInScriptDirectory(consoleConnection, "scripts", "lib-strings", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-stackrng", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-props", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-lmgr", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-edit", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-gui", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-editor", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-match", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-mesg", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-mesgbox", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-look", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-reflist", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-index", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-case", null);
            ReadInScriptDirectory(consoleConnection, "scripts", "lib-mpi", null);*/

            //ReadInScriptDirectory(consoleConnection, "scripts", "lib-*.muf", null);
            //ReadInScriptDirectory(consoleConnection, "scripts", "cmd-*.muf", "cmd-");

            Console.Out.WriteLine("Starting server");
            server.Start(cts.Token);

            while (!cts.IsCancellationRequested)
            {
                /*/
                var input = System.Console.ReadLine();
                if (string.Compare("QUIT", input) == 0)
                {
                    cts.Cancel();
                }*/
                System.Threading.Thread.Sleep(100);
            }
        }

        private static HumanPlayer LoadSandbox(CancellationToken cancellationToken)
        {
            var aether = Room.Make("The Aether", Dbref.GOD);
            aether.id = Dbref.AETHER;
            aether.internalDescription = "You see a massless, infinite black void stretching forever in all directions and across all time.";
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

        /* private static void LoadScriptDirectory(string scriptDirectoryPath, string scriptSearchPattern, string scriptFilePrefix = null)
         {
             foreach (var file in System.IO.Directory.GetFiles(scriptDirectoryPath, scriptSearchPattern))
             {
                 var commandName = file.Substring(scriptDirectoryPath.Length + 1);
                 if (scriptFilePrefix != null)
                     commandName = commandName.Replace(scriptFilePrefix, "");
                 commandName = commandName.Replace(".muf", "");
                 var script = Server.RegisterScript(commandName, LoadScriptFile(file));
                 Console.Out.WriteLine($"Created new script {script.UnparseObject()}");
             }
         }
         */

        private static string LoadScriptFile(string path) => System.IO.File.ReadAllText(path);

        private static void ReadInScriptDirectory(PlayerConnection connection, string scriptDirectoryPath, string scriptSearchPattern, string scriptFilePrefix = null)
        {
            foreach (var file in System.IO.Directory.GetFiles(scriptDirectoryPath, scriptSearchPattern).OrderBy(f => f))
            {
                ReadInScriptFile(file, connection);
                connection.sendOutput($"Read in script file {file}");
            }
        }

        private static void ReadInScriptFile(string path, PlayerConnection connection)
        {
            var lines = System.IO.File.ReadAllLines(path);

            connection.ReceiveInput(lines);
        }
    }
}