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
        public static bool IsInPack(this Pawn pawn)
        {
            bool isInPack = Packmanager.GetLastActivePackmanager.TryGetPackForPawn(pawn, out Pack _);

            Log.Message($"Pawn: {pawn.NameFullColored} is in Pack: {isInPack}");

            return isInPack;
        }
    }
}
