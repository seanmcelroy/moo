using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace moo.common.Models
{
    public abstract class Player : Thing
    {
        public Dbref Home
        {
            get => this.LinkTargets.DefaultIfEmpty(Dbref.AETHER).FirstOrDefault();
            set => this.SetLinkTargets(new[] { value });
        }

        public override Dbref Owner => this.id;

        protected Player() => type = (int)Dbref.DbrefObjectType.Player;

        public async Task<Dbref> FindThingForThisPlayerAsync(string s, CancellationToken cancellationToken)
        {
            if (string.Compare("me", s, true) == 0)
                return this.id;
            if (string.Compare("here", s, true) == 0)
                return this.Location;

            if (s.StartsWith("$"))
            {
                var prop = await GetPropertyPathValueAsync($"_reg/{s[1..]}", cancellationToken);
                if (prop.value == null || prop.Equals(default(Property)))
                    return Dbref.NOT_FOUND;
                if (prop.Type != Property.PropertyType.DbRef)
                    return Dbref.AMBIGUOUS;
                return (Dbref)prop.value;
            }

            if (Dbref.TryParse(s, out Dbref dbref))
                return dbref;

            var inventoryMatches = Dbref.NOT_FOUND; // TODO: await this.MatchAsync(s, cancellationToken);
            Dbref locationMatches = Dbref.NOT_FOUND;

            /*var loc = await ThingRepository.GetLocation(this.id, cancellationToken);
            if (loc != null)
            {
                locationMatches = await loc.MatchAsync(s, cancellationToken);
            }*/

            return inventoryMatches | locationMatches;
        }

        public override void SetLinkTargets(IEnumerable<Dbref> targets)
        {
            if (targets == null || targets.Count() != 1)
                throw new System.ArgumentOutOfRangeException(nameof(targets), "Players can only link to one target, their home");
            base.SetLinkTargets(targets);
        }
    }
}