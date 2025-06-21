using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public class SharedRoomThought : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            Packmanager.GetLastActivePackmanager.TryGetPackForPawn(p, out Pack pack);
            if (pack == null)
                return ThoughtState.Inactive;
            if (p.GetRoom() == null) return ThoughtState.Inactive;
            if (pack.Members.All(x => x.GetRoom() == p.GetRoom())) return ThoughtState.ActiveAtStage(1);

            return ThoughtState.ActiveAtStage(0);
        }
    }
}
