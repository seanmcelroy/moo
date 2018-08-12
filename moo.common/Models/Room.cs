using System;
using System.Collections.Generic;

public class Room : Container {

    public Dbref DropTo = Dbref.NOT_FOUND;

    public Room()
    {
        this.type = (int)Dbref.DbrefObjectType.Room;
    }

    public static Room Make(string name) {
        var room = ThingRepository.Make<Room>();
        room.name = name;
        room.DropTo = room.id;
        Console.WriteLine($"Created new room {name}({room.id})");
        return room;
    }
}