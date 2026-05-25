using System.Linq;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Online
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
             * ONLINE ( -- d ... i )
             * Returns a dbref for every player connected to the server, and lastly the number of connections. 
             */
            var dbrefs = Server.GetConnectedPlayers()
                               .Select(p => new ForthDatum(p.Dbref))
                               .ToList();


            foreach (var dbref in dbrefs)
                parameters.Stack.Push(new ForthDatum(dbref.Value, ForthDatum.DatumType.DbRef));

            parameters.Stack.Push(new ForthDatum(dbrefs.Count));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}