public class Exit : Thing
{
    public Dbref LinkTo = Dbref.NOT_FOUND;

    public override Dbref Link => LinkTo;

    public Exit()
    {
        this.type = (int)Dbref.DbrefObjectType.Exit;
    }
}