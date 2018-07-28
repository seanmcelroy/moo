using System;
using System.Text;
using System.Threading.Tasks;

public class ConsolePlayer : HumanPlayer {

    private readonly StringBuilder buffer = new StringBuilder();

    public static Player make(string name, Container location) {
        var player = ThingRepository.Make<ConsolePlayer>();
        player.name = name;
        player.location = location.id;
        Console.WriteLine($"Created new player {name}(#{player.id})");
        Player.playerConnected(player);
        return player;
    }    

    public override void receiveInput(string input) {
        buffer.Append(input);
    }

    public override Task<CommandResult> popCommand() {
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

    public override async Task sendOutput(string output) {
        await Console.Out.WriteLineAsync(output);
    }
}