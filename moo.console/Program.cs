using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using moo.common;
using moo.common.Connections;
using moo.common.Models;

namespace moo.console
{
    class Program
    {
        static async Task Main()
        {
            Console.Out.WriteLine("\r\n\r\nMoo!\r\n");

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = false;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                    });
            });
            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("Greetings Professor Faulkin.");

            var cts = new CancellationTokenSource();

            try
            {
                var server = Server.Initialize(logger);

                var consolePlayer = await LoadSandbox(logger, cts.Token);
                var consoleConnection = server.AttachConsolePlayer(consolePlayer, Console.In, Console.Out, cts.Token);

                logger.LogInformation("Starting server");
                server.Start(logger, cts.Token);

                logger.LogInformation("Loading script directory");
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
                    Task.Run(async () =>
                    {
                        await ReadInScriptFile(scriptPath, consoleConnection);
                        while (!consoleConnection.IsIdle)
                        {
                            Thread.Sleep(250);
                        }
                        logger.LogDebug("Read in script file {scriptPath}", scriptPath);
                    }).Wait();
                }

                logger.LogInformation("Starting main process loop.");
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
                logger?.LogCritical(ex, "General exception on Main()");
                Environment.Exit(ex.HResult);
            }
        }

        private static async Task<HumanPlayer> LoadSandbox(ILogger? logger, CancellationToken cancellationToken)
        {
            var aether = Room.Make("The Aether", Dbref.GOD, logger);
            aether.id = Dbref.AETHER;
            HostPlayer.make("God", aether);
            var player = HumanPlayer.make("Wizard", aether);
            player.SetFlag(Thing.Flag.WIZARD);

            Console.Out.WriteLine($"Created new player {player.UnparseObjectInternal()}");
            _ = await player.MoveToAsync(aether, cancellationToken);

            return player;
        }

        private static async Task ReadInScriptFile(string path, PlayerConnection connection)
        {
            var lines = await System.IO.File.ReadAllLinesAsync(path);
            connection.ReceiveInputUnattended(lines);
        }
    }
}