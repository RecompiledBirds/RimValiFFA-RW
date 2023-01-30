using HarmonyLib;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiFFARW.Packs
{
    public static class PackDebugActions
    {
        [DebugAction("RimValiFFARW", "Make a new random Pack", allowedGameStates = AllowedGameStates.IsCurrentlyOnMap)]
        public static void MakeRandomPack()
        {
            Map currentMap = Find.CurrentMap;
            Packmanager packmanager = Packmanager.GetLastActivePackmanager;

            if (!currentMap.IsPlayerHome) return;

            List<Pawn> colonists = currentMap.mapPawns.FreeColonistsSpawned.Where(pawn => !pawn.IsInPack()).ToList();
            PackDef def = DefDatabase<PackDef>.GetRandom();
            colonists.TruncateToLength(def.MaxSize);

            bool madeAPack = Pack.TryMakeNewPackFromPawns(def, colonists, false, out Pack pack);
            if (!madeAPack) return;

            packmanager.AddPack(pack);
        }
    }
}
