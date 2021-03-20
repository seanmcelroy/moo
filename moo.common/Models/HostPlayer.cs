using System;

namespace moo.common.Models
{
    public class HostPlayer : Player
    {
        public static HostPlayer make(string name, Thing location)
        {
            var host = ThingRepository.Instance.Make<HostPlayer>();
            host.name = name;
            host.Location = location.id;
            host.Home = location.id;
            Console.WriteLine($"Created new host {host.UnparseObjectInternal()}");
            return host;
        }
    }
}