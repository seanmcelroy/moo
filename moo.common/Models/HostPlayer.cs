using System;
using System.Threading.Tasks;

public class HostPlayer : Player{

    public static HostPlayer make(string name, Container location) {
        var host = ThingRepository.Make<HostPlayer>();
        host.name = name;
        host.location = location.id;
        host.Home = location.id;
        Console.WriteLine($"Created new host {name}({host.id})");
        return host;
    }
}