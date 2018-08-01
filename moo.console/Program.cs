using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;

namespace moo.console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\r\n\r\nMoo!\r\n");
            var cts = new CancellationTokenSource();

            var server = new Server(Console.Out);

            var consolePlayer = LoadSandbox(cts.Token);
            server.AttachConsolePlayer(consolePlayer, Console.In, Console.Out);

            Console.Out.WriteLine("Loading built-in actions");
            foreach (var action in new Action[] { new @Load(), new Look(), new @Save() })
                server.RegisterBuiltInAction(action);

            Console.Out.WriteLine("Loading script directory");
            var scriptDirectoryPath = "scripts";
            var scriptSearchPattern = "cmd-*.muf";
            var scriptFilePrefix = "cmd-";
            foreach (var file in System.IO.Directory.GetFiles(scriptDirectoryPath, scriptSearchPattern))
            {
                var commandName = file.Substring(scriptDirectoryPath.Length + 1).Replace(scriptFilePrefix, "").Replace(".muf", "");
                var script = server.RegisterScript(commandName, LoadScriptFile(file));
                Console.WriteLine($"Created new script {script.name}({script.id})");
            }

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
            var aether = Room.Make("The Aether");
            aether.internalDescription = "You see a massless, infinite black void stretching forever in all directions and across all time.";
            HostPlayer.make("God", aether);
            var player = HumanPlayer.make("Wizard", aether);

            Console.WriteLine($"Created new player {player.name}({player.id})");

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

        private static string LoadScriptFile(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
    }
}