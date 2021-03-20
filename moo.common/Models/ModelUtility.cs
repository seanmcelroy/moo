//#define GOD_PRIV
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;

namespace moo.common.Models
{
    public static class ModelUtility
    {

        public static async Task<bool> Controls(this Dbref whoDbref, Dbref whatDbref, CancellationToken cancellationToken) =>
             await (await whatDbref.Get(cancellationToken)).IsControlledBy(whoDbref, cancellationToken);

        public static async Task<bool> Controls(this Dbref whoDbref, Thing what, CancellationToken cancellationToken) =>
            await what.IsControlledBy(whoDbref, cancellationToken);

        public static async Task<bool> Controls(this PlayerConnection connection, Dbref whatDbref, CancellationToken cancellationToken) =>
            await (await whatDbref.Get(cancellationToken)).IsControlledBy(connection.Dbref, cancellationToken);

        public static async Task<bool> Controls(this PlayerConnection connection, Thing what, CancellationToken cancellationToken) =>
            await what.IsControlledBy(connection.Dbref, cancellationToken);

        public static async Task<bool> IsControlledBy(this Thing what, Dbref whoDbref, CancellationToken cancellationToken) =>
            await what.IsControlledBy(await whoDbref.Get(cancellationToken), cancellationToken);

        public static async Task<bool> IsControlledBy(this Dbref whatDbref, PlayerConnection connection, CancellationToken cancellationToken) =>
            await (await whatDbref.Get(cancellationToken)).IsControlledBy(connection.GetPlayer(), cancellationToken);

        public static async Task<bool> IsControlledBy(this Thing? what, Thing? who, CancellationToken cancellationToken)
        {
            /*
            You control anything you own.
            A wizard or God controls everything.
            Anybody controls an unlinked exit, even if it is locked. Builders should beware of 3, lest their exits be linked or stolen.
            Players control all exits which are linked to their areas, to better facilitate border control.
            If an object is set CHOWN_OK, anyone may @chown object=me and gain control of the object.
            */

            // No one controls invalid objects
            if (what == null || !what.id.IsValid())
                return false;

            if (who == null || !who.id.IsValid())
                return false;

            // Wizard controls everything
            if (who.HasFlag(Thing.Flag.WIZARD))
            {
#if GOD_PRIV
                // Only God controls God's objects
                if (what.Owner.IsGod() && !who.IsGod())
                    return false;
                else
#endif
                return true;
            }

            // Owners control their own stuff
            if (who.id == what.Owner)
                return true;

            return false;

            //return (test_lock_false_default(NOTHING, who, what, MESGPROP_OWNLOCK));
        }

        public static async Task<Dbref> GetLocation(this Dbref dbref, CancellationToken cancellationToken)
        {
            if (!dbref.IsValid())
                return Dbref.NOT_FOUND;
            var thing = await ThingRepository.Instance.GetAsync<Thing>(dbref, cancellationToken);
            if (!thing.isSuccess || thing.value == null)
                return Dbref.NOT_FOUND;

            var locationDbRef = thing.value.Location;
            return locationDbRef;
        }

        public static async Task<Dbref> GetOwner(this Dbref dbref, CancellationToken cancellationToken)
        {
            if (!dbref.IsValid())
                return Dbref.NOT_FOUND;
            var thing = await ThingRepository.Instance.GetAsync<Thing>(dbref, cancellationToken);
            if (!thing.isSuccess || thing.value == null)
                return Dbref.NOT_FOUND;

            return thing.value.Owner;
        }

        public static async Task<Thing?> Get(this Dbref dbref, CancellationToken cancellationToken)
        {
            var result = await ThingRepository.Instance.GetAsync<Thing>(dbref, cancellationToken);
            if (!result.isSuccess || result.value == null)
                return null;

            return result.value;
        }
    }
}