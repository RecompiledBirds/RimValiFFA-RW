using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Source.Harmony
{
    public static class BedUtilityPatch
    {
        public static void Postfix(Pawn pawn1, Pawn pawn2, ref bool __result)
        {
            __result = __result || pawn1.def == RVFFA_Defs.RVFFA_Avali && pawn2.def == RVFFA_Defs.RVFFA_Avali;
        }
    }
}
