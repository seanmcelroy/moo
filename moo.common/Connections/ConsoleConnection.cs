using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class ConsoleConnection : PlayerConnection
{

    private readonly StringBuilder buffer = new StringBuilder();

    private readonly TextReader input;

    private readonly TextWriter output;

    private readonly Task consoleTask;

    public ConsoleConnection(HumanPlayer player, TextReader input, TextWriter output) : base(player)
    {
        this.output = output;

        consoleTask = Task.Factory.StartNew(async () =>
            {
                await output.WriteLineAsync("Starting console interface");
                do
                {
                    var text = await input.ReadLineAsync();
                    receiveInput(text + "\r\n");
                } while (true);
            });
    }

    public override void receiveInput(string input)
    {
        buffer.Append(input);
    }

    public override Task<CommandResult> popCommand()
    {
        if (buffer.Length < 2)
            return Task.FromResult(default(CommandResult));

        var bufferString = buffer.ToString();
        int firstBreak = bufferString.IndexOf("\r\n");
        if (firstBreak == -1)
            return Task.FromResult(default(CommandResult));

        var raw = bufferString.Substring(0, firstBreak);
        buffer.Remove(0, raw.Length + 2);
        return Task.FromResult(new CommandResult(raw));
    }

    public override async Task sendOutput(string output)
    {
        await Console.Out.WriteLineAsync(output);
    }
}