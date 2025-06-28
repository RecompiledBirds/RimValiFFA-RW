using HarmonyLib;
using LudeonTK;
using RimWorld;
using RVCRestructured;
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

            List<Pawn> colonists = currentMap.mapPawns.FreeColonistsSpawned.InRandomOrder().Where(pawn => !pawn.IsInPack() && pawn.IsPackable()).ToList();
            PackDef def = DefDatabase<PackDef>.GetRandom();
            colonists.TruncateToLength(def.MaxSize);

            if (!Pack.TryMakeNewPackFromPawns(def, colonists, false, false, out Pack? pack)) return;

            packmanager.AddPack(pack);
        }

        [DebugAction("RimValiFFARW", "Reset Pack Loss Progression for all Pawns", allowedGameStates = AllowedGameStates.Playing)]
        public static void ResetPackLoss()
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsAndWorld_Alive)
            {
                if (!pawn.TryGetPackInfoContainer(out PackInfoContainer? container)) continue;
                container.TriggerUpdate();
            }
        }
        [DebugAction("RimValiFFARW", "Print list of packs", allowedGameStates = AllowedGameStates.IsCurrentlyOnMap)]
        public static void PrintPackList() => VineLog.Log(Packmanager.GetLastActivePackmanager.PacksReadOnly.Join(pack => pack.NameColored));
    }
}
