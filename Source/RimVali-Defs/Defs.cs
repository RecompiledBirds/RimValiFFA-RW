using RimWorld;
using RVCRestructured.RVR;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW
{
    [DefOf]
    public static class RVFFA_Defs
    {

        [AllowNull]
        public static ThingDef RVFFA_Avali;
        [AllowNull]
        public static ThoughtDef RVFFA_AvaliSleptAlone;
        [AllowNull]
        public static ThoughtDef RVFFA_AvaliSleptNotAlone;
        [AllowNull]
        public static ThoughtDef RVFFA_AvaliSleptWithPack;
        [AllowNull]
        public static TraitDef RVFFA_PackBroken;
        [AllowNull]
        public static HediffDef RVFFA_PackReplacement;
    }
}
