using System;

public class Room : Container
{
    public Dbref DropTo = Dbref.NOT_FOUND;

    public Dbref Parent => this.location;

    public Room()
    {
        this.type = (int)Dbref.DbrefObjectType.Room;
    }

    public static Room Make(string name, Dbref owner)
    {
        var room = ThingRepository.Make<Room>();
        room.name = name;
        room.DropTo = room.id;
        room.owner = owner;
        Console.WriteLine($"Created new room {room.UnparseObject()}");
        return room;
    }
}