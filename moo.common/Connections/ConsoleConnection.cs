using System.IO;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Models;

namespace moo.common.Connections
{
    public class ConsoleConnection : PlayerConnection
    {
        private readonly TextReader input;

        private readonly TextWriter output;

        private readonly Task consoleTask;

        public ConsoleConnection(HumanPlayer player, TextReader input, TextWriter output, CancellationToken cancellationToken) : base(player)
        {
            this.input = input;
            this.output = output;

            consoleTask = Task.Factory.StartNew(async () =>
                {
                    await output.WriteLineAsync("Starting console interface");
                    do
                    {
                        var text = await input.ReadLineAsync();
                        ReceiveInput(text + "\r\n");
                    } while (!cancellationToken.IsCancellationRequested);
                }, cancellationToken);
        }

        public override async Task SendOutput(string output) => await this.output.WriteLineAsync(output);
    }
}