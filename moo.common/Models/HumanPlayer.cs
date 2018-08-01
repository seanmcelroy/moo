using System;
using System.Text;
using System.Threading.Tasks;

public class HumanPlayer : Player
{
    public static HumanPlayer make(string name, Container location)
    {
        var player = ThingRepository.Make<HumanPlayer>();
        player.name = name;
        player.location = location.id;
        return player;
    }
}