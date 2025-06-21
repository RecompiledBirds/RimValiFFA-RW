using HarmonyLib;
using LudeonTK;
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

            List<Pawn> colonists = currentMap.mapPawns.FreeColonistsSpawned.InRandomOrder().Where(pawn => !pawn.IsInPack() && pawn.IsAvali()).ToList();
            PackDef def = DefDatabase<PackDef>.GetRandom();
            colonists.TruncateToLength(def.MaxSize);

            if (!Pack.TryMakeNewPackFromPawns(def, colonists, false, false, out Pack? pack)) return;

            packmanager.AddPack(pack);
        }

        [DebugAction("RimValiFFARW", "Print list of packs", allowedGameStates = AllowedGameStates.IsCurrentlyOnMap)]
        public static void PrintPackList() => RVCLog.Log(Packmanager.GetLastActivePackmanager.PacksReadOnly.Join(pack => pack.NameColored));
    }
}
