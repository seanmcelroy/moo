namespace moo.common.Models
{
    public class HumanPlayer : Player
    {
        public static HumanPlayer make(string name, Thing location)
        {
            var player = ThingRepository.Instance.Make<HumanPlayer>();
            player.name = name;
            player.Home = location.id;
            player.Location = location.id;
            return player;
        }
    }
}