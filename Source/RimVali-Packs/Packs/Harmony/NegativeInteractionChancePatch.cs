using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs;

public static class NegativeInteractionChancePatch
{
    public static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
    {
        if (!initiator.IsInPack(out Pack? initatorPack)) return;
        if (!recipient.IsInPack(out Pack? recipientPack)) return;
        if (initatorPack != recipientPack) return;
        __result /= 3;
    }
}
