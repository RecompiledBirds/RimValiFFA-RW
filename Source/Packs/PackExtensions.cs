using RVCRestructured;
using RVCRestructured.RVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public static class PackExtensions
    {
        /// <summary>
        ///     Checks if a given <see cref="Pawn"/> <paramref name="pawn"/> is a member in any given <see cref="Pack"/>.
        ///     The <see cref="Pack"/>s checked are handled by the <see cref="Packmanager"/>.
        ///     <see cref="Pack"/>s not handled by the <see cref="Packmanager"/> are not considered.
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> to be checked</param>
        /// <returns>If the <see cref="Pawn"/> <paramref name="pawn"/> is part of a <see cref="Pack"/></returns>
        public static bool IsInPack(this Pawn pawn)
        {
            bool isInPack = Packmanager.GetLastActivePackmanager.TryGetPackForPawn(pawn, out Pack _);

            //RVCLog.Log($"Pawn: {pawn.NameFullColored} is in Pack: {isInPack}");

            return isInPack;
        }

        /// <summary>
        ///     Checks if a <see cref="Pawn"/> is an Avali
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static bool IsAvali(this Pawn pawn) => pawn.def is RaceDef raceDef && raceDef.defName == "RVFFA_Avali";
    }
}
