//#define GOD_PRIV
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;

namespace moo.common.Models
{
    public static class ModelUtility
    {

        public static async Task<bool> Controls(this Dbref whoDbref, Dbref whatDbref, CancellationToken cancellationToken) => await whatDbref.IsControlledBy(whoDbref, cancellationToken);

        public static async Task<bool> Controls(this Dbref whoDbref, Thing what, CancellationToken cancellationToken) => await what.id.IsControlledBy(whoDbref, cancellationToken);

        public static async Task<bool> Controls(this PlayerConnection connection, Dbref whatDbref, CancellationToken cancellationToken) => await whatDbref.IsControlledBy(connection.Dbref, cancellationToken);

        public static async Task<bool> Controls(this PlayerConnection connection, Thing what, CancellationToken cancellationToken) => await what.id.IsControlledBy(connection.Dbref, cancellationToken);

        public static async Task<bool> IsControlledBy(this Thing what, Dbref whoDbref, CancellationToken cancellationToken) => await what.id.IsControlledBy(whoDbref, cancellationToken);

        public static async Task<bool> IsControlledBy(this Thing what, PlayerConnection connection, CancellationToken cancellationToken) => await what.id.IsControlledBy(connection.Dbref, cancellationToken);

        public static async Task<bool> IsControlledBy(this Dbref whatDbref, Dbref whoDbref, CancellationToken cancellationToken)
        {
            /*
            You control anything you own.
            A wizard or God controls everything.
            Anybody controls an unlinked exit, even if it is locked. Builders should beware of 3, lest their exits be linked or stolen.
            Players control all exits which are linked to their areas, to better facilitate border control.
            If an object is set CHOWN_OK, anyone may @chown object=me and gain control of the object.
            */

            // No one controls invalid objects
            if (!whatDbref.IsValid())
                return false;

            if (!whoDbref.IsValid())
                return false;

            var what = await ThingRepository.Instance.GetAsync<Thing>(whoDbref, cancellationToken);
            if (!what.isSuccess || what.value == null)
                return false;

            var who = await ThingRepository.Instance.GetAsync<Thing>(whoDbref, cancellationToken);
            if (!who.isSuccess || who.value == null)
                return false;

            // Wizard controls everything
            if (who.value.HasFlag(Thing.Flag.WIZARD))
            {
#if GOD_PRIV
                // Only God controls God's objects
                if (what.value.Owner.IsGod() && !who.value.IsGod())
                    return false;
                else
#endif
                return true;
            }

            // Owners control their own stuff
            if (whoDbref == what.value.Owner)
                return true;

            return false;

            //return (test_lock_false_default(NOTHING, who, what, MESGPROP_OWNLOCK));
        }

        public static async Task<Thing?> GetLocation(this Dbref dbref, CancellationToken cancellationToken)
        {
            if (!dbref.IsValid())
                return null;
            var thing = await ThingRepository.Instance.GetAsync<Thing>(dbref, cancellationToken);
            if (!thing.isSuccess || thing.value == null)
                return null;

            var locationDbRef = thing.value.Location;
            var location = await ThingRepository.Instance.GetAsync<Thing>(locationDbRef, cancellationToken);
            if (!location.isSuccess || location.value == null)
                return null;

            return location.value;
        }

        public static async Task<Thing?> GetOwner(this Dbref dbref, CancellationToken cancellationToken)
        {
            if (!dbref.IsValid())
                return null;
            var thing = await ThingRepository.Instance.GetAsync<Thing>(dbref, cancellationToken);
            if (!thing.isSuccess || thing.value == null)
                return null;

            var ownerDbRef = thing.value.Owner;
            var owner = await ThingRepository.Instance.GetAsync<Thing>(ownerDbRef, cancellationToken);
            if (!owner.isSuccess || owner.value == null)
                return null;

            return owner.value;
        }

    }
}