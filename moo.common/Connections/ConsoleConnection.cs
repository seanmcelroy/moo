using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ConsoleConnection : PlayerConnection
{
    private readonly TextReader input;

    private readonly TextWriter output;

    private readonly Task consoleTask;

    public ConsoleConnection(HumanPlayer player, TextReader input, TextWriter output, CancellationToken cancellationToken) : base(player)
    {
        this.output = output;

        consoleTask = Task.Factory.StartNew(async () =>
            {
                await output.WriteLineAsync("Starting console interface");
                do
                {
                    var text = await input.ReadLineAsync();
                    ReceiveInput(text + "\r\n");
                } while (true);
            }, cancellationToken);
    }

    public override async Task sendOutput(string output) => await this.output.WriteLineAsync(output);
}