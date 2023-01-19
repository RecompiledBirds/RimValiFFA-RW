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

            List<Pawn> colonists = currentMap.mapPawns.FreeColonistsSpawned.ListFullCopy();
            colonists.TruncateToLength(5);

            bool madeAPack = Pack.TryMakeNewPackFromPawns(DefDatabase<PackDef>.GetRandom(), colonists, false, out Pack pack);
            if (!madeAPack)
            {
                RVCLog.Log("Could not make a Pack!", RVCLogType.Error);
                return;
            }

            packmanager.AddPack(pack);

            RVCLog.Log($"Made a Pack: {madeAPack} using {colonists.Join()}");
        }
    }
}
