using System;
using System.Collections.Generic;

public class Room : Container {


    public static Room Make(string name) {
        var room = ThingRepository.Make<Room>();
        room.name = name;
        Console.WriteLine($"Created new room {name}({room.id})");
        return room;
    }
}