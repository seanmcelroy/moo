using System;
using System.Threading.Tasks;
using System.Threading;

public abstract class Player : Container
{
    public Dbref Home = Dbref.NOT_FOUND;

    public override Dbref[] Links => new Dbref[] { Home };

    public override Dbref Owner => this.id;

    protected Player()
    {
        this.type = (int)Dbref.DbrefObjectType.Player;
    }

    public async Task<Dbref> FindThingForThisPlayerAsync(string s, CancellationToken cancellationToken)
    {
        if (String.Compare("me", s, true) == 0)
            return this.id;
        if (String.Compare("here", s, true) == 0)
            return this.location;

        if (s.StartsWith("$"))
        {
            var prop = await GetPropertyPathValueAsync($"_reg/{s.Substring(1)}", cancellationToken);
            if (prop.value == null || prop.Equals(default(Property)))
                return Dbref.NOT_FOUND;
            if (prop.Type != Property.PropertyType.DbRef)
                return Dbref.AMBIGUOUS;
            return (Dbref)prop.value;
        }

        if (Dbref.TryParse(s, out Dbref dbref))
            return dbref;

        var inventoryMatches = await this.MatchAsync(s, cancellationToken);
        Dbref locationMatches = Dbref.NOT_FOUND;

        var locationLookup = await ThingRepository.GetAsync<Container>(this.location, cancellationToken);
        if (locationLookup.isSuccess && locationLookup.value != null)
        {
            var loc = locationLookup.value;
            locationMatches = await loc.MatchAsync(s, cancellationToken);
        }

        return inventoryMatches | locationMatches;
    }

    public override void SetLinkTargets(params Dbref[] targets)
    {
        if (targets == null || targets.Length != 1)
            throw new System.ArgumentOutOfRangeException(nameof(targets), "Players can only link to one target, their home");

        this.Home = targets[0];
    }
}