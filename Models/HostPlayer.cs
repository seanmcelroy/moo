using System;
using System.Threading.Tasks;

public class HostPlayer : Player{

    public static Player make(string name, Container location) {
        HostPlayer host = ThingRepository.Make<HostPlayer>();
        host.name = name;
        host.location = location.id;
        Console.WriteLine($"Created new host {name}(#{host.id})");
        return host;
    }    

    public override void receiveInput(string input) {

    }

    public override Task<CommandResult> popCommand() {
        return Task.FromResult(default(CommandResult));
    }

    public override Task sendOutput(string output) {
        return Task.CompletedTask;
    }
}