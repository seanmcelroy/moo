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

        public override void SetLinkTargets(IEnumerable<Dbref> targets)
        {
            if (targets == null || targets.Count() != 1)
                throw new System.ArgumentOutOfRangeException(nameof(targets), "Players can only link to one target, their home");
            base.SetLinkTargets(targets);
        }
    }
}