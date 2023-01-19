using System;
using Microsoft.Extensions.Logging;

namespace moo.common.Models
{
    public class Room : Thing
    {
        public Dbref DropTo = Dbref.NOT_FOUND;

        public Dbref Parent => this.Location;

        public Room() => type = (int)Dbref.DbrefObjectType.Room;

        public static Room Make(string name, Dbref owner, ILogger? logger)
        {
            var room = ThingRepository.Instance.Make<Room>();
            room.name = name;
            room.DropTo = room.id;
            room.owner = owner;
            logger?.LogDebug("Created new room {unparsed}", room.UnparseObjectInternal());
            return room;
        }
    }
}