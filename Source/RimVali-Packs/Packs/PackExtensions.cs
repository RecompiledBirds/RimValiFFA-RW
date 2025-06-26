using RVCRestructured;
using RVCRestructured.RVR;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public static class PackExtensions
    {
        public static bool CanJoinAPack(this Pawn pawn)
        {
            return !(pawn.story.traits.HasTrait(RVFFA_Defs.RVFFA_PackBroken) || pawn.health.hediffSet.HasHediff(RVFFA_Defs.RVFFA_PackReplacement));
        }
        /// <summary>
        ///     Checks if a given <see cref="Pawn"/> <paramref name="pawn"/> is a member in any given <see cref="Pack"/>.
        ///     The <see cref="Pack"/>s checked are handled by the <see cref="Packmanager"/>.
        ///     <see cref="Pack"/>s not handled by the <see cref="Packmanager"/> are not considered.
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> to be checked</param>
        /// <returns>If the <see cref="Pawn"/> <paramref name="pawn"/> is part of a <see cref="Pack"/></returns>
        public static bool IsInPack(this Pawn pawn)
        {

            return pawn.IsInPack(out Pack? _);
        }


        /// <summary>
        ///     Checks if a given <see cref="Pawn"/> <paramref name="pawn"/> is a member in any given <see cref="Pack"/>.
        ///     The <see cref="Pack"/>s checked are handled by the <see cref="Packmanager"/>.
        ///     <see cref="Pack"/>s not handled by the <see cref="Packmanager"/> are not considered.
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> to be checked</param>
        /// <param name="pack">The <see cref="Pack"/> the pawn is part of</param>
        /// <returns>If the <see cref="Pawn"/> <paramref name="pawn"/> is part of a <see cref="Pack"/></returns>
        public static bool IsInPack(this Pawn pawn, [NotNullWhen(true)] out Pack? pack)
        {
            bool isInPack = Packmanager.GetLastActivePackmanager.TryGetPackForPawn(pawn, out pack);


            return isInPack;
        }
        /// <summary>
        ///     Checks if a <see cref="Pawn"/> is an Avali
        /// </summary>B
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static bool IsAvali(this Pawn pawn) =>  pawn.def.defName == "RVFFA_Avali";
    }
}
