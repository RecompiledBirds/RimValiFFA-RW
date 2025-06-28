using System.Diagnostics.CodeAnalysis;
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
            return Packmanager.GetLastActivePackmanager.TryGetPackForPawn(pawn, out pack);

        }

        public static bool GetPackInfoComp(this Pawn pawn, [NotNullWhen(true)] out PackInfoComp? comp)
        {
            return Packmanager.GetLastActivePackmanager.GetCachedCompFor(pawn, out comp);
        }
        /// <summary>
        ///     Checks if a <see cref="Pawn"/> is an Avali
        /// </summary>B
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static bool IsPackable(this Pawn pawn) => pawn.TryGetPackInfoContainer(out PackInfoContainer? _);
        public static bool TryGetPackInfoContainer(this Pawn pawn, [NotNullWhen(true)] out PackInfoContainer? packInfoContainer)
        {
            packInfoContainer = null;
            if (ModsConfig.BiotechActive)
            {
                PackGene? packGene = (PackGene?)pawn.genes.GenesListForReading.FirstOrFallback(x => x is PackGene, null);
                packInfoContainer = packGene?.PackInfoContainer;
                return packGene!=null;
            }
            if (pawn.GetPackInfoComp(out PackInfoComp? comp))
            {
                packInfoContainer = comp.PackInfoContainer;
                return true;
            }
            return false;
        }
    }
}
