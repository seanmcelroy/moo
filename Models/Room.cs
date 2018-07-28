using System;
using System.Collections.Generic;

public class Room : Container {


    public static Room make(string name) {
        Room room = ThingRepository.Make<Room>();
        room.name = name;
        Console.WriteLine($"Created new room {name}(#{room.id})");
        return room;
    }
}