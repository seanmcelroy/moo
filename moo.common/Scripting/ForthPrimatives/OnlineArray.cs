using System.Linq;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class OnlineArray
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            // ONLINE_ARRAY ( -- a )
            // Returns a single stack item, which is a list array, containing a dbref for every player connected to the server. 
            var dbrefs = Server.GetConnectedPlayers()
                               .Select(p => new ForthDatum(p.Dbref))
                               .ToList();
            parameters.Stack.Push(new ForthDatum(new ForthListArray(dbrefs)));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}