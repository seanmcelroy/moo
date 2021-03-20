using System;

namespace moo.common.Models
{
    public class Room : Thing
    {
        public Dbref DropTo = Dbref.NOT_FOUND;

        public Dbref Parent => this.Location;

        public Room() => type = (int)Dbref.DbrefObjectType.Room;

        public static Room Make(string name, Dbref owner)
        {
            var room = ThingRepository.Instance.Make<Room>();
            room.name = name;
            room.DropTo = room.id;
            room.owner = owner;
            Console.WriteLine($"Created new room {room.UnparseObjectInternal()}");
            return room;
        }
    }
}