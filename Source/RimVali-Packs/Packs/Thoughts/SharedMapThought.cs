using RimValiFFARW.Packs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public class SharedMapThought : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.IsInPack(out Pack? pack))
                return ThoughtState.Inactive;
            if (p.GetRoom() == null) return ThoughtState.Inactive;
            if (pack.Members.Any(x => x.Map == p.Map)) return ThoughtState.ActiveAtStage(1);

            return ThoughtState.ActiveAtStage(0);
        }
    }
}
