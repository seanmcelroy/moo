using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Models.Dbref;

namespace moo.common.Database
{
    public sealed class MatchResult
    {
        public Thing MatchFrom { get; init; }
        public Player Player { get; init; }
        public string MatchName { get; init; }
        public DbrefObjectType PreferredType { get; init; } = DbrefObjectType.Unknown;
        public CancellationToken CancellationToken { get; init; }

        internal bool CheckKeys { get; set; }

        public Dbref ExactMatch { get; internal set; } = Dbref.NOT_FOUND;

        public Dbref LastMatch { get; internal set; } = Dbref.NOT_FOUND;

        public int MatchCount { get; internal set; }

        private readonly List<Dbref> _matches = new();

        public ImmutableList<Dbref> Matches
        {
            get => _matches.ToImmutableList();
        }

        private MatchResult(Player player, string matchName, DbrefObjectType preferredType, CancellationToken cancellationToken)
        {
            MatchFrom = player;
            Player = player;
            MatchName = matchName;
            PreferredType = preferredType;
            CancellationToken = cancellationToken;
        }

        internal static MatchResult InitObjectSearch(Player player, string matchName, DbrefObjectType preferredType, CancellationToken cancellationToken)
        {
            return new MatchResult(player, matchName, preferredType, cancellationToken);
        }

        internal static MatchResult InitRemoteSearch(Player player, Thing matchFrom, string matchName, DbrefObjectType preferredType, CancellationToken cancellationToken)
        {
            var result = new MatchResult(player, matchName, preferredType, cancellationToken)
            {
                MatchFrom = matchFrom
            };
            return result;
        }

        public async Task<Dbref> ChooseOne(Dbref thing1, Dbref thing2)
        {
            // If one or the other is unset, that makes it easy.
            if (thing1 == NOT_FOUND)
                return thing2;
            if (thing2 == NOT_FOUND)
                return thing1;

            // Check for type preference 
            if (PreferredType != DbrefObjectType.Unknown)
            {
                if (thing1.Type == PreferredType)
                {
                    if (thing2.Type == PreferredType)
                        return thing1;
                }
                else if (thing2.Type == PreferredType)
                    return thing2;
            }

            // Do we want to check if the objects are locked against the player?
            if (this.CheckKeys)
            {
                var has1 = true; // TODO could_doit(descr, md->match_who, thing1);
                var has2 = true; // TODO could_doit(descr, md->match_who, thing2);

                if (has1 && !has2)
                    return thing1;
                else if (has2 && !has1)
                    return thing2;
                /* else fall through */
            }

            // Try environment distance, pick the closer one if applicable.
            int d1 = await Dbref.EnvironmentalDistance(MatchFrom.id, thing1, this.CancellationToken);
            int d2 = await Dbref.EnvironmentalDistance(MatchFrom.id, thing2, this.CancellationToken);

            if (d1 < d2) return thing1;
            else if (d2 < d1) return thing2;

            // All else fails, toss a coin.
            return Scripting.ForthPrimatives.RandomMethods.Random(0, 10) % 2 == 0 ? thing1 : thing2;
        }
    }
}